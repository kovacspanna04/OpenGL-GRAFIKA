﻿using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace LAB1
{
    internal static class Program
    {
        private static IWindow graphicWindow;

        private static GL Gl;

        private static uint program;

        private static readonly string VertexShaderSource = @"
        #version 330 core
        layout (location = 0) in vec3 vPos;
		layout (location = 1) in vec4 vCol;

		out vec4 outCol;
        
        void main()
        {
			outCol = vCol;
            // gl_Position = vec4(vPos.x, vPos.y, vPos.z, 1.0);
            gl_Position = vec4(vPos.x * 2.0, vPos.y * 3.0, vPos.z * 4.0, 1.0);   // nagyobb, deformalt abra
        }
        ";


        private static readonly string FragmentShaderSource = @"
        #version 330 core
        out vec4 FragColor;
		
		in vec4 outCol;

        void main()
        {
            FragColor = outCol;
        }
        ";

        static void Main(string[] args)
        {
            WindowOptions windowOptions = WindowOptions.Default;
            windowOptions.Title = "1. szeminarium - haromszog";
            windowOptions.Size = new Silk.NET.Maths.Vector2D<int>(500, 500);

            graphicWindow = Window.Create(windowOptions);

            graphicWindow.Load += GraphicWindow_Load;
            graphicWindow.Update += GraphicWindow_Update;
            graphicWindow.Render += GraphicWindow_Render;
            graphicWindow.Closing += GraphicWindow_Closing;

            graphicWindow.Run();
        }

        private static void GraphicWindow_Closing()
        {
            Gl.DeleteProgram(program);
        }

        private static void GraphicWindow_Load()
        {
            // egszeri beallitasokat
            //Console.WriteLine("Loaded");

            Gl = graphicWindow.CreateOpenGL();

            Gl.Enable(EnableCap.CullFace);          // haromszog egyik oldalanak kirajzolasa(a hata ne latszodjon)
            Gl.CullFace(TriangleFace.Back);

            Gl.ClearColor(System.Drawing.Color.White);

            uint vshader = Gl.CreateShader(ShaderType.VertexShader);
            uint fshader = Gl.CreateShader(ShaderType.FragmentShader);

            // ha ezekbol kiszedtem akkor kidobta a lenti exceptiont
            Gl.ShaderSource(vshader, VertexShaderSource); 
            Gl.CompileShader(vshader);
            Gl.GetShader(vshader, ShaderParameterName.CompileStatus, out int vStatus);
            if (vStatus != (int)GLEnum.True)
                throw new Exception("Vertex shader failed to compile: " + Gl.GetShaderInfoLog(vshader));

            Gl.ShaderSource(fshader, FragmentShaderSource);
            Gl.CompileShader(fshader);    // ha ezt leviszem a vegere hiba: OpenGL ERROR at Gl.UseProgram: InvalidOperation

            program = Gl.CreateProgram();    // hiba : Error linking shader
            //OpenGL ERROR at Vertex Buffer: InvalidValue
            Gl.AttachShader(program, vshader);
            Gl.AttachShader(program, fshader);   // ha ezt kiszedem fekete abra + OpenGL ERROR at Vertex Buffer: InvalidOperation
            Gl.LinkProgram(program);     // nelkule hiba: OpenGL ERROR at Gl.UseProgram: InvalidOperation
            Gl.DetachShader(program, vshader);
            Gl.DetachShader(program, fshader);
            Gl.DeleteShader(vshader);
            Gl.DeleteShader(fshader);
            //Gl.CompileShader(fshader);

            Gl.GetProgram(program, GLEnum.LinkStatus, out var status);
            if (status == 0)
            {
                Console.WriteLine($"Error linking shader {Gl.GetProgramInfoLog(program)}");
            }

        }

        private static void GraphicWindow_Update(double deltaTime)
        {
            // NO GL
            // make it threadsave
            //Console.WriteLine($"Update after {deltaTime} [s]");
        }

        private static void CheckGLError(string location)
        {
            var error = Gl.GetError();
            if (error != GLEnum.NoError)
            {
                Console.WriteLine($"OpenGL ERROR at {location}: {error}");
            }
        }


        private static unsafe void GraphicWindow_Render(double deltaTime)
        {
            //Console.WriteLine($"Render after {deltaTime} [s]");

            Gl.Clear(ClearBufferMask.ColorBufferBit);

            uint vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);

            float[] vertexArray = new float[] {
                -0.5f, -0.5f, 0.0f,
                +0.5f, -0.5f, 0.0f,
                 0.0f, +0.5f, 0.0f,
                 1f, 1f, 0f
            };

            float[] colorArray = new float[] {
                1.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 1.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 1.0f, 1.0f,
                1.0f, 0.0f, 0.0f, 1.0f,
            };

            uint[] indexArray = new uint[] {
                0, 1, 2,
                //2, 1, 3
                3, 1, 2            // forditott sorrend
            };

            uint vertices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, vertices);    // ha ezt kitorlom, akkor nem rajzol ki semmit
            // a BindBuffer -t barmivel kicserelem nem rajzol ki semmit tobbet
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)vertexArray.AsSpan(), GLEnum.StaticDraw);    // ha ezt kiszedem nem rajzol ki semmit
            //Gl.BindBuffer(GLEnum.ArrayBuffer, vertices);
            CheckGLError("Vertex Buffer");

            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, null);    // e nelkul sem rajzol ki semmit
            Gl.EnableVertexAttribArray(0);   // enelkul sem
            //Gl.EnableVertexAttribArray(5);    // ha modositottam szintugy nem rajzol ki semmit
            CheckGLError("Vertex Attribute Pointer");   // position

            uint colors = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, colors);    // e nelkul semmi sem jelenik meg
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)colorArray.AsSpan(), GLEnum.StaticDraw);   // e nelkul egy fekete abra jelenik meg csak
            CheckGLError("Color Buffer");

            Gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, null);    // e nelkul szinten egy fekete abra lesz
            Gl.EnableVertexAttribArray(1);
            CheckGLError("Vertex Attribute Pointer");   // color

            uint indices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, indices);
            Gl.BufferData(GLEnum.ElementArrayBuffer, (ReadOnlySpan<uint>)indexArray.AsSpan(), GLEnum.StaticDraw);    // ha ezt torlom akkor sem rajzol ki semmit
            CheckGLError("Index Buffer");

            Gl.BindBuffer(GLEnum.ArrayBuffer, 0);

            Gl.UseProgram(program);     // e nelkul nem rajzol ki semmit
            CheckGLError("Gl.UseProgram");


            Gl.DrawElements(GLEnum.Triangles, (uint)indexArray.Length, GLEnum.UnsignedInt, null); // we used element buffer
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);
            Gl.BindVertexArray(vao);
            CheckGLError("Gl.DrawElements");

            // always unbound the vertex buffer first, so no halfway results are displayed by accident
            Gl.DeleteBuffer(vertices);
            Gl.DeleteBuffer(colors);
            Gl.DeleteBuffer(indices);
            Gl.DeleteVertexArray(vao);
        }
    }
}
