using System;
using System.Collections.Generic;

namespace MonoGameLibrary.WFC.Core;

public class WFCOverlappingModel<T> where T : struct
{
    private readonly int _N;
    private readonly int _MX, _MY;
    private readonly bool _periodicOutput;

    private readonly List<byte[]> _patterns;
    private readonly List<T> _tileValues;
    private readonly double[] _weights;
    private readonly int _T; // pattern count

    private readonly int[][][] _propagator; // [direction][pattern] -> compatible pattern indices

    private bool[][] _wave;
    private int[][][] _compatible;
    private int[] _observed;

    private (int, int)[] _stack;
    private int _stackSize;

    private double[] _weightLogWeights;
    private double[] _distribution;
    private int[] _sumsOfOnes;
    private double[] _sumsOfWeights;
    private double[] _sumsOfWeightLogWeights;
    private double[] _entropies;
    private double _sumOfWeights;
    private double _sumOfWeightLogWeights;
    private double _startingEntropy;

    private static readonly int[] DX = { -1, 0, 1, 0 };
    private static readonly int[] DY = { 0, 1, 0, -1 };
    private static readonly int[] Opposite = { 2, 3, 0, 1 };

    public int PatternCount => _T;

    public WFCOverlappingModel(T[,] input, int N, int outputWidth, int outputHeight,
        bool periodicInput = false, bool periodicOutput = false, int symmetry = 8)
    {
        _N = N;
        _MX = outputWidth;
        _MY = outputHeight;
        _periodicOutput = periodicOutput;

        int SX = input.GetLength(0);
        int SY = input.GetLength(1);

        // Map input T values to byte indices
        var tileToIndex = new Dictionary<T, byte>();
        _tileValues = new List<T>();

        var sample = new byte[SX * SY];
        for (int y = 0; y < SY; y++)
        {
            for (int x = 0; x < SX; x++)
            {
                T value = input[x, y];
                if (!tileToIndex.TryGetValue(value, out byte index))
                {
                    index = (byte)_tileValues.Count;
                    tileToIndex[value] = index;
                    _tileValues.Add(value);
                }
                sample[x + y * SX] = index;
            }
        }

        int C = _tileValues.Count;

        // Pattern helper functions
        static byte[] PatternFromFunc(Func<int, int, byte> f, int n)
        {
            var result = new byte[n * n];
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                    result[x + y * n] = f(x, y);
            return result;
        }

        static byte[] Rotate(byte[] p, int n) =>
            PatternFromFunc((x, y) => p[n - 1 - y + x * n], n);

        static byte[] Reflect(byte[] p, int n) =>
            PatternFromFunc((x, y) => p[n - 1 - x + y * n], n);

        static long Hash(byte[] p, int c)
        {
            long result = 0, power = 1;
            for (int i = 0; i < p.Length; i++)
            {
                result += p[p.Length - 1 - i] * power;
                power *= c;
            }
            return result;
        }

        // Extract patterns
        _patterns = new List<byte[]>();
        var patternIndices = new Dictionary<long, int>();
        var weightList = new List<double>();

        int xmax = periodicInput ? SX : SX - N + 1;
        int ymax = periodicInput ? SY : SY - N + 1;

        for (int y = 0; y < ymax; y++)
        {
            for (int x = 0; x < xmax; x++)
            {
                var ps = new byte[8][];
                ps[0] = PatternFromFunc((dx, dy) => sample[(x + dx) % SX + (y + dy) % SY * SX], N);
                ps[1] = Reflect(ps[0], N);
                ps[2] = Rotate(ps[0], N);
                ps[3] = Reflect(ps[2], N);
                ps[4] = Rotate(ps[2], N);
                ps[5] = Reflect(ps[4], N);
                ps[6] = Rotate(ps[4], N);
                ps[7] = Reflect(ps[6], N);

                for (int k = 0; k < symmetry; k++)
                {
                    byte[] p = ps[k];
                    long h = Hash(p, C);
                    if (patternIndices.TryGetValue(h, out int index))
                    {
                        weightList[index] += 1;
                    }
                    else
                    {
                        patternIndices.Add(h, weightList.Count);
                        weightList.Add(1.0);
                        _patterns.Add(p);
                    }
                }
            }
        }

        _weights = weightList.ToArray();
        _T = _weights.Length;

        // Build propagator using overlap agreement
        static bool Agrees(byte[] p1, byte[] p2, int dx, int dy, int n)
        {
            int xmin = dx < 0 ? 0 : dx;
            int xmax = dx < 0 ? dx + n : n;
            int ymin = dy < 0 ? 0 : dy;
            int ymax = dy < 0 ? dy + n : n;
            for (int y = ymin; y < ymax; y++)
                for (int x = xmin; x < xmax; x++)
                    if (p1[x + n * y] != p2[x - dx + n * (y - dy)])
                        return false;
            return true;
        }

        _propagator = new int[4][][];
        for (int d = 0; d < 4; d++)
        {
            _propagator[d] = new int[_T][];
            for (int t = 0; t < _T; t++)
            {
                var list = new List<int>();
                for (int t2 = 0; t2 < _T; t2++)
                    if (Agrees(_patterns[t], _patterns[t2], DX[d], DY[d], N))
                        list.Add(t2);
                _propagator[d][t] = list.ToArray();
            }
        }
    }

    public bool Run(int seed, int limit = -1)
    {
        if (_wave == null) Init();
        Clear();

        var random = new Random(seed);

        for (int l = 0; l < limit || limit < 0; l++)
        {
            int node = NextUnobservedNode(random);
            if (node >= 0)
            {
                Observe(node, random);
                bool success = Propagate();
                if (!success) return false;
            }
            else
            {
                for (int i = 0; i < _wave.Length; i++)
                    for (int t = 0; t < _T; t++)
                        if (_wave[i][t])
                        {
                            _observed[i] = t;
                            break;
                        }
                return true;
            }
        }

        return true;
    }

    public T[,] GetOutput()
    {
        var result = new T[_MX, _MY];

        if (_observed[0] >= 0)
        {
            for (int y = 0; y < _MY; y++)
            {
                int dy = y < _MY - _N + 1 ? 0 : _N - 1;
                for (int x = 0; x < _MX; x++)
                {
                    int dx = x < _MX - _N + 1 ? 0 : _N - 1;
                    int patternIndex = _observed[x - dx + (y - dy) * _MX];
                    result[x, y] = _tileValues[_patterns[patternIndex][dx + dy * _N]];
                }
            }
        }

        return result;
    }

    private void Init()
    {
        int cellCount = _MX * _MY;
        _wave = new bool[cellCount][];
        _compatible = new int[cellCount][][];
        for (int i = 0; i < cellCount; i++)
        {
            _wave[i] = new bool[_T];
            _compatible[i] = new int[_T][];
            for (int t = 0; t < _T; t++)
                _compatible[i][t] = new int[4];
        }

        _distribution = new double[_T];
        _observed = new int[cellCount];

        _weightLogWeights = new double[_T];
        _sumOfWeights = 0;
        _sumOfWeightLogWeights = 0;

        for (int t = 0; t < _T; t++)
        {
            _weightLogWeights[t] = _weights[t] * Math.Log(_weights[t]);
            _sumOfWeights += _weights[t];
            _sumOfWeightLogWeights += _weightLogWeights[t];
        }

        _startingEntropy = Math.Log(_sumOfWeights) - _sumOfWeightLogWeights / _sumOfWeights;

        _sumsOfOnes = new int[cellCount];
        _sumsOfWeights = new double[cellCount];
        _sumsOfWeightLogWeights = new double[cellCount];
        _entropies = new double[cellCount];

        _stack = new (int, int)[cellCount * _T];
        _stackSize = 0;
    }

    private void Clear()
    {
        for (int i = 0; i < _wave.Length; i++)
        {
            for (int t = 0; t < _T; t++)
            {
                _wave[i][t] = true;
                for (int d = 0; d < 4; d++)
                    _compatible[i][t][d] = _propagator[Opposite[d]][t].Length;
            }

            _sumsOfOnes[i] = _T;
            _sumsOfWeights[i] = _sumOfWeights;
            _sumsOfWeightLogWeights[i] = _sumOfWeightLogWeights;
            _entropies[i] = _startingEntropy;
            _observed[i] = -1;
        }

        _stackSize = 0;
    }

    private int NextUnobservedNode(Random random)
    {
        double min = 1E+4;
        int argmin = -1;

        for (int i = 0; i < _wave.Length; i++)
        {
            if (!_periodicOutput && (i % _MX + _N > _MX || i / _MX + _N > _MY))
                continue;

            int remainingValues = _sumsOfOnes[i];
            double entropy = _entropies[i];

            if (remainingValues > 1 && entropy <= min)
            {
                double noise = 1E-6 * random.NextDouble();
                if (entropy + noise < min)
                {
                    min = entropy + noise;
                    argmin = i;
                }
            }
        }

        return argmin;
    }

    private void Observe(int node, Random random)
    {
        bool[] w = _wave[node];
        for (int t = 0; t < _T; t++)
            _distribution[t] = w[t] ? _weights[t] : 0.0;

        int r = WeightedRandom(_distribution, random.NextDouble());
        for (int t = 0; t < _T; t++)
            if (w[t] != (t == r))
                Ban(node, t);
    }

    private bool Propagate()
    {
        while (_stackSize > 0)
        {
            (int i1, int t1) = _stack[_stackSize - 1];
            _stackSize--;

            int x1 = i1 % _MX;
            int y1 = i1 / _MX;

            for (int d = 0; d < 4; d++)
            {
                int x2 = x1 + DX[d];
                int y2 = y1 + DY[d];

                if (!_periodicOutput && (x2 < 0 || y2 < 0 || x2 + _N > _MX || y2 + _N > _MY))
                    continue;

                if (x2 < 0) x2 += _MX;
                else if (x2 >= _MX) x2 -= _MX;
                if (y2 < 0) y2 += _MY;
                else if (y2 >= _MY) y2 -= _MY;

                int i2 = x2 + y2 * _MX;
                int[] p = _propagator[d][t1];
                int[][] compat = _compatible[i2];

                for (int l = 0; l < p.Length; l++)
                {
                    int t2 = p[l];
                    int[] comp = compat[t2];

                    comp[d]--;
                    if (comp[d] == 0) Ban(i2, t2);
                }
            }
        }

        return _sumsOfOnes[0] > 0;
    }

    private void Ban(int i, int t)
    {
        _wave[i][t] = false;

        int[] comp = _compatible[i][t];
        for (int d = 0; d < 4; d++) comp[d] = 0;
        _stack[_stackSize] = (i, t);
        _stackSize++;

        _sumsOfOnes[i]--;
        _sumsOfWeights[i] -= _weights[t];
        _sumsOfWeightLogWeights[i] -= _weightLogWeights[t];

        double sum = _sumsOfWeights[i];
        _entropies[i] = Math.Log(sum) - _sumsOfWeightLogWeights[i] / sum;
    }

    private static int WeightedRandom(double[] weights, double r)
    {
        double sum = 0;
        for (int i = 0; i < weights.Length; i++) sum += weights[i];
        double threshold = r * sum;

        double partialSum = 0;
        for (int i = 0; i < weights.Length; i++)
        {
            partialSum += weights[i];
            if (partialSum >= threshold) return i;
        }
        return 0;
    }
}
