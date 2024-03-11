using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Celeste.Mod.BetterRefillGemsPlus
{
    public static class ImageRecolor
    {
        static int unique = 0;
        static Dictionary<(Atlas, string), Task<MTexture>> result = [];
        static Dictionary<(Atlas, string), MTexture> resultcomplete = [];
        public static MTexture MTextureDrawOutline(MTexture mtex)
        {
            VirtualTexture? nvtex = null;
            try
            {
                var orig = mtex.Texture.Texture_Safe;
                var rect = mtex.ClipRect;
                var fmt = orig.Format;
                //var sin = stackalloc byte[16];
                BetterRefillGemsPlusModule.loadimmediately.TryAdd(Environment.CurrentManagedThreadId, null);
                nvtex = new VirtualTexture(Environment.CurrentManagedThreadId.ToString() + nameof(BetterRefillGemsPlus) + (++unique).ToString(), rect.Width, rect.Height, Color.Transparent);
                bool outrange(int v1, int v2)
                {
                    return !(v1 >= 0 && v1 < rect.Width && v2 < rect.Height && v2 >= 0);
                }
                void calcTyped<T>(T sin, T transparent) where T : struct
                {
                    T[] b = new T[1 * rect.Width * rect.Height];
                    orig.GetData(0, rect, b, 0, b.Length);
                    for (int i = 0; i < rect.Height; i++)
                    {
                        for (int j = 0; j < rect.Width; j++)
                        {
                            if (!transparent.Equals(b[((i * rect.Width + j) * 1)]))
                            {
                                if
                                (
                                    (outrange(i + 1, j) || transparent.Equals(b[(((i + 1) * rect.Width + j) * 1)])) ||
                                    (outrange(i, j - 1) || transparent.Equals(b[(((i) * rect.Width + j - 1) * 1)])) ||
                                    (outrange(i - 1, j) || transparent.Equals(b[(((i - 1) * rect.Width + j) * 1)])) ||
                                    (outrange(i, j + 1) || transparent.Equals(b[(((i) * rect.Width + j + 1) * 1)]))
                                )
                                {
                                    int k = 0;
                                    {
                                        b[(i * rect.Width + j) * 1 + k] = sin;
                                    }
                                }
                            }
                        }
                    }
                    nvtex.Texture_Safe.SetData(b);
                }
                void calcBigEndian(byte[] sin, byte[] transparent)
                {
                    sin = sin.Reverse().ToArray();
                    transparent = transparent.Reverse().ToArray();
                    var size = sin.Length;
                    byte[] b = new byte[size * rect.Width * rect.Height];
                    orig.GetData(0, rect, b, 0, b.Length);
                    for (int i = 0; i < rect.Height; i++)
                    {
                        for (int j = 0; j < rect.Width; j++)
                        {
                            if (!transparent.SequenceEqual(b.Skip((i * rect.Width + j) * size).Take(size)))
                            {
                                if
                                (//trust jit.
                                    (outrange(i + 1, j) || transparent.SequenceEqual(b.Skip(((i + 1) * rect.Width + j) * size).Take(size))) ||
                                    (outrange(i, j - 1) || transparent.SequenceEqual(b.Skip(((i) * rect.Width + j - 1) * size).Take(size))) ||
                                    (outrange(i - 1, j) || transparent.SequenceEqual(b.Skip(((i - 1) * rect.Width + j) * size).Take(size))) ||
                                    (outrange(i, j + 1) || transparent.SequenceEqual(b.Skip(((i) * rect.Width + j + 1) * size).Take(size)))
                                )
                                {
                                    for (int k = 0; k < size; k++)
                                    {
                                        b[(i * rect.Width + j) * size + k] = sin[k];
                                    }
                                }
                            }
                        }
                    }
                    nvtex.Texture_Safe.SetData(b);
                }
                Color col = new(255, 41, 41, 255);
                Color trans = new(0, 0, 0, 0);
                if (fmt != Microsoft.Xna.Framework.Graphics.SurfaceFormat.Color)
                {
                    Logger.Log(LogLevel.Error, nameof(BetterRefillGemsPlus), "Looks loke you have found a image in different format. Send it to me so I could add support for it.\n"
                        + $"Metxture:{mtex.Atlas.DataPath} {mtex.AtlasPath}");
                }
                switch (fmt)
                {
                    case Microsoft.Xna.Framework.Graphics.SurfaceFormat.Color:
                        calcTyped(col, trans);
                        break;
                    //todo: I don't have image in below format.
                    case Microsoft.Xna.Framework.Graphics.SurfaceFormat.Vector4:
                        {
                            calcTyped(col.ToVector4(), trans.ToVector4());
                        }
                        break;
                    //omg they're all big endian
                    //oh, just reverse them in func
                    case Microsoft.Xna.Framework.Graphics.SurfaceFormat.Bgra4444:
                        calcBigEndian([0, 255], [0, 0]);
                        break;
                    case Microsoft.Xna.Framework.Graphics.SurfaceFormat.Bgra5551:
                        calcBigEndian([0, 63], [0, 0]);
                        break;
                    case Microsoft.Xna.Framework.Graphics.SurfaceFormat.ColorBgraEXT:
                        calcBigEndian([0, 0, 255, 255], [0, 0, 0, 0]);
                        break;
                    //case Microsoft.Xna.Framework.Graphics.SurfaceFormat.ColorSrgbEXT:
                    //    //What's this?
                    //    break;
                    case Microsoft.Xna.Framework.Graphics.SurfaceFormat.HalfVector4:
                        var oh = BitConverter.HalfToUInt16Bits((Half)1.0);
                        var ohb = BitConverter.GetBytes(oh);
                        var zh = BitConverter.HalfToUInt16Bits((Half)0.0);
                        var zhb = BitConverter.GetBytes(zh);
                        calcBigEndian([.. ohb, .. ohb, .. zhb, .. zhb], [.. zhb, .. zhb, .. zhb, .. zhb,]);
                        break;
                    case Microsoft.Xna.Framework.Graphics.SurfaceFormat.Rgba1010102:
                        calcBigEndian([0, 0, 15, 255], [0, 0, 0, 0,]);
                        break;
                    case Microsoft.Xna.Framework.Graphics.SurfaceFormat.Rgba64:
                        calcBigEndian([255, 255, 0, 0, 0, 0, 255, 255], [0, 0, 0, 0, 0, 0, 0, 0,]);
                        break;
                    default:
                        Logger.Log(LogLevel.Warn, nameof(BetterRefillGemsPlus), $"failed when loading {mtex.Atlas.DataPath} {mtex.AtlasPath} . unsupported surface format {fmt}.");
                        return mtex;
                }
                return new MTexture(nvtex)
                {
                    Atlas = mtex.Atlas,
                    AtlasPath = mtex.AtlasPath,
                    Center = mtex.Center,
                    //BottomUV=mtex.BottomUV,
                    DrawOffset = mtex.DrawOffset,
                    //LeftUV=mtex.LeftUV,

                };

            }
            catch (Exception ex)
            {
                nvtex?.Dispose();
                Logger.Log(nameof(BetterRefillGemsPlus), $"Something went wrong when trying to create {mtex.Atlas.DataPath} {mtex.AtlasPath} .\nException: {ex.Message}\n{ex.StackTrace}");
                return mtex;
            }
            finally
            {
                BetterRefillGemsPlusModule.loadimmediately.TryRemove(Environment.CurrentManagedThreadId, out _);
            }
        }


        public static Task<MTexture> MarkResourceImageAsync(string s, Atlas atlas = null)
        {
            var task = Task.Run(() => MarkResourceImage(s, atlas));
            result.TryAdd((atlas, s), task);
            return task;
        }
        public static MTexture MarkResourceImage(string s, Atlas atlas = null)
        {
            atlas ??= GFX.Game;
            var mtex = atlas[s];
            return MTextureDrawOutline(mtex);
        }
        public static void MarkResourceSpriteAsync(string s, Atlas atlas = null, params string[] path)
        {
            atlas ??= GFX.Game;
            foreach (var u in path)
            {
                foreach (var v in atlas.GetAtlasSubtextures(s + u))
                {
                    result.TryAdd((atlas, v.AtlasPath), Task.Run(() => MTextureDrawOutline(v)));
                }
            }
        }

        public static MTexture GetImage(MTexture mtex)
        {
            return GetImage(mtex.Atlas, mtex.AtlasPath);
        }
        public static MTexture GetImage(Atlas atlas, string v)
        {
            if (result.Remove((atlas, v), out var task))
            {
                task.Wait();
                return resultcomplete[(atlas, v)] = task.Result;
            }
            if (resultcomplete.TryGetValue((atlas, v), out var res))
            {
                return res;
            }
            return resultcomplete[(atlas, v)] = MarkResourceImage(v, atlas);
        }
    }
}
