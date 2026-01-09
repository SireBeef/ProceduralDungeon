using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGameLibrary.Graphics;

public class TextureAtlas
{
    private Dictionary<string, TextureRegion> _regions;
    private Dictionary<string, Animation> _animations;

    /// <summary>
    /// Gets or Sets the source texture represented by this texture atlas.
    /// </summary>
    public Texture2D Texture { get; set; }

    /// <summary>
    /// Creates a new texture atlas.
    /// </summary>
    public TextureAtlas()
    {
        _regions = new Dictionary<string, TextureRegion>();
        _animations = new Dictionary<string, Animation>();
    }

    /// <summary>
    /// Creates a new texture atlas instance using the given texture.
    /// </summary>
    /// <param name="texture">The source texture represented by the texture atlas.</param>
    public TextureAtlas(Texture2D texture)
    {
        Texture = texture;
        _regions = new Dictionary<string, TextureRegion>();
        _animations = new Dictionary<string, Animation>();
    }

    /// <summary>
    /// Creates a new region and adds it to this texture atlas.
    /// </summary>
    /// <param name="name">The name to give the texture region.</param>
    /// <param name="x">The top-left x-coordinate position of the region boundary relative to the top-left corner of the source texture boundary.</param>
    /// <param name="y">The top-left y-coordinate position of the region boundary relative to the top-left corner of the source texture boundary.</param>
    /// <param name="width">The width, in pixels, of the region.</param>
    /// <param name="height">The height, in pixels, of the region.</param>
    public void AddRegion(string name, int x, int y, int width, int height)
    {
        TextureRegion region = new TextureRegion(Texture, x, y, width, height);
        _regions.Add(name, region);
    }

    /// <summary>
    /// Gets the region from this texture atlas with the specified name.
    /// </summary>
    /// <param name="name">The name of the region to retrieve.</param>
    /// <returns>The TextureRegion with the specified name.</returns>
    public TextureRegion GetRegion(string name)
    {
        return _regions[name];
    }

    /// <summary>
    /// Removes the region from this texture atlas with the specified name.
    /// </summary>
    /// <param name="name">The name of the region to remove.</param>
    /// <returns></returns>
    public bool RemoveRegion(string name)
    {
        return _regions.Remove(name);
    }

    /// <summary>
    /// Removes all regions from this texture atlas.
    /// </summary>
    public void Clear()
    {
        _regions.Clear();
    }

    /// <summary>
    /// Creates a new texture atlas based on a texture atlas JSON configuration file.
    /// </summary>
    /// <param name="content">The content manager used to load the texture for the atlas.</param>
    /// <param name="fileName">The path to the json file, relative to the content root directory.</param>
    /// <returns>The texture atlas created by this method.</returns>
    public static TextureAtlas FromFile(ContentManager content, string fileName)
    {
        TextureAtlas atlas = new TextureAtlas();

        string filePath = Path.Combine(content.RootDirectory, fileName);

        using (Stream stream = TitleContainer.OpenStream(filePath))
        {
            TextureAtlasJson data = JsonSerializer.Deserialize<TextureAtlasJson>(
                stream,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (data == null)
            {
                throw new InvalidDataException("Invalid texture atlas JSON.");
            }

            // The "texture" property contains the content path for the Texture2D to load.
            // So we will retrieve that value then use the content manager to load the texture.
            atlas.Texture = content.Load<Texture2D>(data.Texture);

            // The "regions" array contains individual region objects, each one describing
            // a different texture region within the atlas.
            //
            // Example:
            // {
            //   "texture": "images/atlas",
            //   "regions": [
            //     { "name": "spriteOne", "x": 0,  "y": 0,  "width": 32, "height": 32 },
            //     { "name": "spriteTwo", "x": 32, "y": 0,  "width": 32, "height": 32 }
            //   ]
            // }
            //
            // So we retrieve all of the region entries then loop through each one
            // and generate a new TextureRegion instance from it and add it to this atlas.
            if (data.Regions != null)
            {
                foreach (var region in data.Regions)
                {
                    if (!string.IsNullOrEmpty(region.Name))
                    {
                        atlas.AddRegion(
                            region.Name,
                            region.X,
                            region.Y,
                            region.Width,
                            region.Height
                        );
                    }
                }
            }

            // The "animations" property contains individual "animation" elements, each one describing
            // a different animation within the atlas.
            //
            // Example:
            // {
            //   "texture": "images/atlas",
            //   "regions": [
            //     { "name": "spriteFrame-01", "x": 0,  "y": 0,  "width": 32, "height": 32 },
            //     { "name": "spriteFrame-02", "x": 32, "y": 0,  "width": 32, "height": 32 }
            //   ]
            //   "animations": [
            //      { region: "spriteFrame-01"}
            //      { region: "spriteFrame-02"}
            //   ]
            // }
            //
            // So we retrieve all of the "animation" elements then loop through each one
            // and generate a new Animation instance from it and add it to this atlas.
            if (data.Animations != null)
            {
                foreach (var animationJson in data.Animations)
                {
                    List<TextureRegion> frames = new List<TextureRegion>();
                    if (!string.IsNullOrEmpty(animationJson.Name))
                    {
                        foreach (var frame in animationJson.Frames)
                        {
                            TextureRegion frameRegion = atlas.GetRegion(frame.Region);
                            frames.Add(frameRegion);
                        }
                    }
                    Animation animationToAdd = new Animation(frames, TimeSpan.FromMilliseconds(animationJson.Delay));
                    atlas._animations.Add(animationJson.Name, animationToAdd);
                }
            }
            return atlas;
        }

    }

    /// <summary>
    /// Creates a new sprite using the region from this texture atlas with the specified name.
    /// </summary>
    /// <param name="regionName">The name of the region to create the sprite with.</param>
    /// <returns>A new Sprite using the texture region with the specified name.</returns>
    public Sprite CreateSprite(string regionName)
    {
        TextureRegion region = GetRegion(regionName);
        return new Sprite(region);
    }

    /// <summary>
    /// Creates a new animated sprite using the animation from this texture atlas with the specified name.
    /// </summary>
    /// <param name="animationName">The name of the animation to use.</param>
    /// <returns>A new AnimatedSprite using the animation with the specified name.</returns>
    public AnimatedSprite CreateAnimatedSprite(string animationName)
    {
        Animation animation = GetAnimation(animationName);
        return new AnimatedSprite(animation);
    }

    /// <summary>
    /// Adds the given animation to this texture atlas with the specified name.
    /// </summary>
    /// <param name="animationName">The name of the animation to add.</param>
    /// <param name="animation">The animation to add.</param>
    public void AddAnimation(string animationName, Animation animation)
    {
        _animations.Add(animationName, animation);
    }

    /// <summary>
    /// Gets the animation from this texture atlas with the specified name.
    /// </summary>
    /// <param name="animationName">The name of the animation to retrieve.</param>
    /// <returns>The animation with the specified name.</returns>
    public Animation GetAnimation(string animationName)
    {
        return _animations[animationName];
    }

    /// <summary>
    /// Removes the animation with the specified name from this texture atlas.
    /// </summary>
    /// <param name="animationName">The name of the animation to remove.</param>
    /// <returns>true if the animation is removed successfully; otherwise, false.</returns>
    public bool RemoveAnimation(string animationName)
    {
        return _animations.Remove(animationName);
    }
    #region JSON DTOs (internal)

    private sealed class TextureAtlasJson
    {
        public string Texture { get; set; }
        public List<TextureRegionJson> Regions { get; set; }
        public List<TextureAnimationJson> Animations { get; set; }
    }

    private sealed class TextureRegionJson
    {
        public string Name { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    private sealed class TextureAnimationJson
    {
        public string Name { get; set; }
        public float Delay { get; set; }
        public List<AnimationFrameJson> Frames { get; set; }
    }

    private sealed class AnimationFrameJson
    {
        public string Region { get; set; }
    }

    #endregion
}
