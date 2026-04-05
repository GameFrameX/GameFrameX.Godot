using System;
using System.Collections.Generic;
using System.Text;
using Godot;
using FairyGUI.Utils;
using System.Linq;

namespace FairyGUI
{
    public class TextMeshCluster
    {
        public class TextMeshInfo
        {
            public Texture2D tex;
            public ArrayMesh mesh;
            public SurfaceTool surfaceTool;
            public int vertexCount = 0;
            public TextMeshInfo()
            {
                mesh = new ArrayMesh();
                surfaceTool = new SurfaceTool();
            }
            public void AddGlyph(Rect glyphRect, Rect uvRect, Color color, Color[] colors, float Italic)
            {
                if (!Mathf.IsZeroApprox(Italic))
                    Italic = glyphRect.height * Mathf.Tan(Italic);
                if (colors != null && colors.Length >= 4)
                {
                    surfaceTool.SetColor(colors[0]);
                    surfaceTool.SetUV(uvRect.leftBottom);
                    surfaceTool.AddVertex(new Vector3(glyphRect.xMin, glyphRect.yMax, 0));
                    surfaceTool.SetColor(colors[1]);
                    surfaceTool.SetUV(uvRect.leftTop);
                    surfaceTool.AddVertex(new Vector3(glyphRect.xMin + Italic, glyphRect.yMin, 0));
                    surfaceTool.SetColor(colors[2]);
                    surfaceTool.SetUV(uvRect.rightTop);
                    surfaceTool.AddVertex(new Vector3(glyphRect.xMax + Italic, glyphRect.yMin, 0));
                    surfaceTool.SetColor(colors[3]);
                    surfaceTool.SetUV(uvRect.rightBottom);
                    surfaceTool.AddVertex(new Vector3(glyphRect.xMax, glyphRect.yMax, 0));
                }
                else
                {
                    surfaceTool.SetColor(color);
                    surfaceTool.SetUV(uvRect.leftBottom);
                    surfaceTool.AddVertex(new Vector3(glyphRect.xMin, glyphRect.yMax, 0));
                    surfaceTool.SetUV(uvRect.leftTop);
                    surfaceTool.AddVertex(new Vector3(glyphRect.xMin + Italic, glyphRect.yMin, 0));
                    surfaceTool.SetUV(uvRect.rightTop);
                    surfaceTool.AddVertex(new Vector3(glyphRect.xMax + Italic, glyphRect.yMin, 0));
                    surfaceTool.SetUV(uvRect.rightBottom);
                    surfaceTool.AddVertex(new Vector3(glyphRect.xMax, glyphRect.yMax, 0));
                }
                surfaceTool.AddIndex(vertexCount);
                surfaceTool.AddIndex(vertexCount + 1);
                surfaceTool.AddIndex(vertexCount + 2);
                surfaceTool.AddIndex(vertexCount);
                surfaceTool.AddIndex(vertexCount + 2);
                surfaceTool.AddIndex(vertexCount + 3);
                vertexCount += 4;
            }
            public void AddGlyph(Vector3[] vertices, Vector2[] uvs, Color color, Color[] colors)
            {
                if (vertices.Length < 4 || uvs.Length < 4)
                {
                    GD.PushError("glyph vertex not enough");
                    return;
                }
                if (colors != null && colors.Length >= 4)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        surfaceTool.SetColor(colors[i]);
                        surfaceTool.SetUV(uvs[i]);
                        surfaceTool.AddVertex(vertices[i]);
                    }
                }
                else
                {
                    surfaceTool.SetColor(color);
                    for (int i = 0; i < 4; i++)
                    {
                        surfaceTool.SetUV(uvs[i]);
                        surfaceTool.AddVertex(vertices[i]);
                    }
                }
                surfaceTool.AddIndex(vertexCount);
                surfaceTool.AddIndex(vertexCount + 1);
                surfaceTool.AddIndex(vertexCount + 2);
                surfaceTool.AddIndex(vertexCount);
                surfaceTool.AddIndex(vertexCount + 2);
                surfaceTool.AddIndex(vertexCount + 3);
                vertexCount += 4;
            }
        }
        public List<TextMeshInfo> meshs = new List<TextMeshInfo>();
        protected static Stack<TextMeshInfo> meshPool = new Stack<TextMeshInfo>();
        public void Clear()
        {
            for (int i = 0; i > meshs.Count; i++)
            {
                TextMeshInfo mesh = meshs[i];
                mesh.tex = null;
                mesh.mesh.ClearSurfaces();
                mesh.surfaceTool.Clear();
                mesh.vertexCount = 0;
                meshPool.Push(mesh);
            }
            meshs.Clear();
        }
        public TextMeshInfo GetMesh(Texture2D tex)
        {
            TextMeshInfo mesh = meshs.Find((info) => { return info.tex != null && info.tex == tex; });
            if (mesh == null)
            {
                mesh = meshs.Find((info) => { return info.tex == null; });
                if (mesh == null)
                {
                    if (meshPool.Count > 0)
                    {
                        mesh = meshPool.Pop();
                    }
                    else
                    {
                        mesh = new TextMeshInfo();
                    }
                    mesh.surfaceTool.Begin(Mesh.PrimitiveType.Triangles);
                    meshs.Add(mesh);
                }
                mesh.tex = tex;
            }
            return mesh;
        }
        public bool AddGlyph(Texture2D tex, Rect glyphRect, Rect uvRect, Color color, Color[] colors, float Italic)
        {
            TextMeshInfo mesh = GetMesh(tex);
            if (mesh != null)
            {
                mesh.AddGlyph(glyphRect, uvRect, color, colors, Italic);
                return true;
            }
            return false;
        }
        public bool AddGlyph(Texture2D tex, Vector3[] vertices, Vector2[] uvs, Color color, Color[] colors)
        {
            TextMeshInfo mesh = GetMesh(tex);
            if (mesh != null)
            {
                mesh.AddGlyph(vertices, uvs, color, colors);
                return true;
            }
            return false;
        }
        public void Finish()
        {
            for (int i = 0; i < meshs.Count; i++)
            {
                TextMeshInfo mesh = meshs[i];
                if (mesh.tex != null)
                {
                    mesh.surfaceTool.Commit(mesh.mesh);
                }
            }
        }
    }
}