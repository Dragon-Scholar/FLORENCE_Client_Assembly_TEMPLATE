using System.Drawing;
using System.Drawing.Imaging;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace FLORENCE_Client.FrameworkSpace.ClientSpace.DataSpace.OutputSpace
{
    class Graphics : GameWindow
    {
        public Graphics()
            : base(512, 512, new GraphicsMode(32, 24, 0, 4))
        {
            System.Console.WriteLine("FLORENCE: Graphics");
        }

        Vector3[] vertdata;
        Vector3[] coldata;
        Vector2[] texcoorddata;
        Vector3[] normdata;

        int[] indicedata;
        int ibo_elements;

        List<FLORENCE_Client.FrameworkSpace.ClientSpace.DataSpace.OutputSpace.GraphicsSpace.Volume> objects = new List<FLORENCE_Client.FrameworkSpace.ClientSpace.DataSpace.OutputSpace.GraphicsSpace.Volume>();
        FLORENCE_Client.FrameworkSpace.ClientSpace.DataSpace.OutputSpace.GraphicsSpace.Camera camera = new FLORENCE_Client.FrameworkSpace.ClientSpace.DataSpace.OutputSpace.GraphicsSpace.Camera();
        FLORENCE_Client.FrameworkSpace.ClientSpace.DataSpace.OutputSpace.GraphicsSpace.Light activeLight = new FLORENCE_Client.FrameworkSpace.ClientSpace.DataSpace.OutputSpace.GraphicsSpace.Light(new Vector3(), new Vector3(0.9f, 0.80f, 0.8f));
        Vector2 lastMousePos = new Vector2();
        Matrix4 view = Matrix4.Identity;

        Dictionary<string, int> textures = new Dictionary<string, int>();
        Dictionary<string, FLORENCE_Client.FrameworkSpace.ClientSpace.DataSpace.OutputSpace.GraphicsSpace.ShaderProgram> shaders = new Dictionary<string, FLORENCE_Client.FrameworkSpace.ClientSpace.DataSpace.OutputSpace.GraphicsSpace.ShaderProgram>();
        string activeShader = "normal";
        Dictionary<String, FLORENCE_Client.FrameworkSpace.ClientSpace.DataSpace.OutputSpace.GraphicsSpace.Material> materials = new Dictionary<string, FLORENCE_Client.FrameworkSpace.ClientSpace.DataSpace.OutputSpace.GraphicsSpace.Material>();

        float time = 0.0f;

        void initProgram()
        {
            lastMousePos = new Vector2(Mouse.X, Mouse.Y);
            CursorVisible = false;
            camera.MouseSensitivity = 0.0025f;

            GL.GenBuffers(1, out ibo_elements);

            // Load shaders from file
            shaders.Add("default", new FLORENCE_Client.FrameworkSpace.ClientSpace.DataSpace.OutputSpace.GraphicsSpace.ShaderProgram("..\\..\\..\\Vertex_Shaders\\vs.glsl", "..\\..\\..\\Fragment_Shaders\\fs.glsl", true));
            shaders.Add("textured", new FLORENCE_Client.FrameworkSpace.ClientSpace.DataSpace.OutputSpace.GraphicsSpace.ShaderProgram("..\\..\\..\\Vertex_Shaders\\vs_tex.glsl", "..\\..\\..\\Fragment_Shaders\\fs_tex.glsl", true));
            shaders.Add("normal", new FLORENCE_Client.FrameworkSpace.ClientSpace.DataSpace.OutputSpace.GraphicsSpace.ShaderProgram("..\\..\\..\\Vertex_Shaders\\vs_norm.glsl", "..\\..\\..\\Fragment_Shaders\\fs_norm.glsl", true));
            shaders.Add("lit", new FLORENCE_Client.FrameworkSpace.ClientSpace.DataSpace.OutputSpace.GraphicsSpace.ShaderProgram("..\\..\\..\\Vertex_Shaders\\vs_lit.glsl", "..\\..\\..\\Fragment_Shaders\\fs_lit.glsl", true));

            activeShader = "lit";

            // Move camera away from origin
            camera.Position += new Vector3(0f, 0f, 3f);
            
            textures.Add("earth.png", loadImage("..\\..\\..\\Textures\\earth.png"));
            FLORENCE_Client.FrameworkSpace.ClientSpace.DataSpace.OutputSpace.GraphicsSpace.ObjVolume earth = FLORENCE_Client.FrameworkSpace.ClientSpace.DataSpace.OutputSpace.GraphicsSpace.ObjVolume.LoadFromFile("..\\..\\..\\Characters\\earth.obj");
            earth.TextureID = textures["earth.png"];
            earth.Position += new Vector3(1f, 1f, -2f);
            earth.Material = new FLORENCE_Client.FrameworkSpace.ClientSpace.DataSpace.OutputSpace.GraphicsSpace.Material(new Vector3(0.15f), new Vector3(1), new Vector3(0.2f), 5);
            objects.Add(earth);
        }

        /// <summary>
        /// Loads all materials in a file, along with their required maps.
        /// Materials will not overwrite existing materials with the same name.
        /// </summary>
        /// <param name="filename">MTL file to load from</param>
        private void loadMaterials(String filename)
        {
            foreach (var mat in FLORENCE_Client.FrameworkSpace.ClientSpace.DataSpace.OutputSpace.GraphicsSpace.Material.LoadFromFile(filename))
            {
                if (!materials.ContainsKey(mat.Key))
                {
                    materials.Add(mat.Key, mat.Value);
                }
            }

            // Load textures
            foreach (FLORENCE_Client.FrameworkSpace.ClientSpace.DataSpace.OutputSpace.GraphicsSpace.Material mat in materials.Values)
            {
                if (File.Exists(mat.AmbientMap) && !textures.ContainsKey(mat.AmbientMap))
                {
                    textures.Add(mat.AmbientMap, loadImage(mat.AmbientMap));
                }

                if (File.Exists(mat.DiffuseMap) && !textures.ContainsKey(mat.DiffuseMap))
                {
                    textures.Add(mat.DiffuseMap, loadImage(mat.DiffuseMap));
                }

                if (File.Exists(mat.SpecularMap) && !textures.ContainsKey(mat.SpecularMap))
                {
                    textures.Add(mat.SpecularMap, loadImage(mat.SpecularMap));
                }

                if (File.Exists(mat.NormalMap) && !textures.ContainsKey(mat.NormalMap))
                {
                    textures.Add(mat.NormalMap, loadImage(mat.NormalMap));
                }

                if (File.Exists(mat.OpacityMap) && !textures.ContainsKey(mat.OpacityMap))
                {
                    textures.Add(mat.OpacityMap, loadImage(mat.OpacityMap));
                }
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            initProgram();

            Title = "FLORENCE: Game";
            GL.ClearColor(Color.CornflowerBlue);
            GL.PointSize(5f);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Viewport(0, 0, Width, Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Enable(EnableCap.DepthTest);

            GL.UseProgram(shaders[activeShader].ProgramID);
            shaders[activeShader].EnableVertexAttribArrays();

            int indiceat = 0;

            // Draw all objects
            foreach (FLORENCE_Client.FrameworkSpace.ClientSpace.DataSpace.OutputSpace.GraphicsSpace.Volume v in objects)
            {
                GL.BindTexture(TextureTarget.Texture2D, v.TextureID);

                GL.UniformMatrix4(shaders[activeShader].GetUniform("modelview"), false, ref v.ModelViewProjectionMatrix);

                if (shaders[activeShader].GetAttribute("maintexture") != -1)
                {
                    GL.Uniform1(shaders[activeShader].GetAttribute("maintexture"), v.TextureID);
                }

                if (shaders[activeShader].GetUniform("view") != -1)
                {
                    GL.UniformMatrix4(shaders[activeShader].GetUniform("view"), false, ref view);
                }

                if (shaders[activeShader].GetUniform("model") != -1)
                {
                    GL.UniformMatrix4(shaders[activeShader].GetUniform("model"), false, ref v.ModelMatrix);
                }

                if (shaders[activeShader].GetUniform("material_ambient") != -1)
                {
                    GL.Uniform3(shaders[activeShader].GetUniform("material_ambient"), ref v.Material.AmbientColor);
                }

                if (shaders[activeShader].GetUniform("material_diffuse") != -1)
                {
                    GL.Uniform3(shaders[activeShader].GetUniform("material_diffuse"), ref v.Material.DiffuseColor);
                }

                if (shaders[activeShader].GetUniform("material_specular") != -1)
                {
                    GL.Uniform3(shaders[activeShader].GetUniform("material_specular"), ref v.Material.SpecularColor);
                }

                if (shaders[activeShader].GetUniform("material_specExponent") != -1)
                {
                    GL.Uniform1(shaders[activeShader].GetUniform("material_specExponent"), v.Material.SpecularExponent);
                }

                if (shaders[activeShader].GetUniform("light_position") != -1)
                {
                    GL.Uniform3(shaders[activeShader].GetUniform("light_position"), ref activeLight.Position);
                }

                if (shaders[activeShader].GetUniform("light_color") != -1)
                {
                    GL.Uniform3(shaders[activeShader].GetUniform("light_color"), ref activeLight.Color);
                }

                if (shaders[activeShader].GetUniform("light_diffuseIntensity") != -1)
                {
                    GL.Uniform1(shaders[activeShader].GetUniform("light_diffuseIntensity"), activeLight.DiffuseIntensity);
                }

                if (shaders[activeShader].GetUniform("light_ambientIntensity") != -1)
                {
                    GL.Uniform1(shaders[activeShader].GetUniform("light_ambientIntensity"), activeLight.AmbientIntensity);
                }

                GL.DrawElements(BeginMode.Triangles, v.IndiceCount, DrawElementsType.UnsignedInt, indiceat * sizeof(uint));
                indiceat += v.IndiceCount;
            }

            shaders[activeShader].DisableVertexAttribArrays();

            GL.Flush();
            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            ProcessInput();

            List<Vector3> verts = new List<Vector3>();
            List<int> inds = new List<int>();
            List<Vector3> colors = new List<Vector3>();
            List<Vector2> texcoords = new List<Vector2>();
            List<Vector3> normals = new List<Vector3>();

            // Assemble vertex and indice data for all volumes
            int vertcount = 0;
            foreach (FLORENCE_Client.FrameworkSpace.ClientSpace.DataSpace.OutputSpace.GraphicsSpace.Volume v in objects)
            {
                verts.AddRange(v.GetVerts().ToList());
                inds.AddRange(v.GetIndices(vertcount).ToList());
                colors.AddRange(v.GetColorData().ToList());
                texcoords.AddRange(v.GetTextureCoords());
                normals.AddRange(v.GetNormals().ToList());
                vertcount += v.VertCount;
            }

            vertdata = verts.ToArray();
            indicedata = inds.ToArray();
            coldata = colors.ToArray();
            texcoorddata = texcoords.ToArray();
            normdata = normals.ToArray();

            GL.BindBuffer(BufferTarget.ArrayBuffer, shaders[activeShader].GetBuffer("vPosition"));

            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(vertdata.Length * Vector3.SizeInBytes), vertdata, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(shaders[activeShader].GetAttribute("vPosition"), 3, VertexAttribPointerType.Float, false, 0, 0);

            // Buffer vertex color if shader supports it
            if (shaders[activeShader].GetAttribute("vColor") != -1)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, shaders[activeShader].GetBuffer("vColor"));
                GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(coldata.Length * Vector3.SizeInBytes), coldata, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(shaders[activeShader].GetAttribute("vColor"), 3, VertexAttribPointerType.Float, true, 0, 0);
            }


            // Buffer texture coordinates if shader supports it
            if (shaders[activeShader].GetAttribute("texcoord") != -1)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, shaders[activeShader].GetBuffer("texcoord"));
                GL.BufferData<Vector2>(BufferTarget.ArrayBuffer, (IntPtr)(texcoorddata.Length * Vector2.SizeInBytes), texcoorddata, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(shaders[activeShader].GetAttribute("texcoord"), 2, VertexAttribPointerType.Float, true, 0, 0);
            }

            if (shaders[activeShader].GetAttribute("vNormal") != -1)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, shaders[activeShader].GetBuffer("vNormal"));
                GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(normdata.Length * Vector3.SizeInBytes), normdata, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(shaders[activeShader].GetAttribute("vNormal"), 3, VertexAttribPointerType.Float, true, 0, 0);
            }

            // Update object positions
            time += (float)e.Time;

            // Update model view matrices
            foreach (FLORENCE_Client.FrameworkSpace.ClientSpace.DataSpace.OutputSpace.GraphicsSpace.Volume v in objects)
            {
                v.CalculateModelMatrix();
                v.ViewProjectionMatrix = camera.GetViewMatrix() * Matrix4.CreatePerspectiveFieldOfView(1.3f, ClientSize.Width / (float)ClientSize.Height, 1.0f, 40.0f); 
                v.ModelViewProjectionMatrix = v.ModelMatrix * v.ViewProjectionMatrix;
            }

            GL.UseProgram(shaders[activeShader].ProgramID);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            // Buffer index data
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo_elements);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indicedata.Length * sizeof(int)), indicedata, BufferUsageHint.StaticDraw);
            
            view = camera.GetViewMatrix();
        }

        /// <summary>
        /// Handles keyboard and mouse input
        /// </summary>
        private void ProcessInput()
        {
            /* We'll use the escape key to make the program easy to exit from.
             * Otherwise it's annoying since the mouse cursor is trapped inside.*/
            if (Keyboard.GetState().IsKeyDown(Key.Escape))
            {
                Exit();
                this.Close();
                this.Dispose(); 
            }

            /** Let's start by adding WASD input (feel free to change the keys if you want,
             * hopefully a later tutorial will have a proper input manager) for translating the camera. */
            if (Keyboard.GetState().IsKeyDown(Key.W))
            {
                camera.Move(0f, 0.1f, 0f);
            }

            if (Keyboard.GetState().IsKeyDown(Key.S))
            {
                camera.Move(0f, -0.1f, 0f);
            }

            if (Keyboard.GetState().IsKeyDown(Key.A))
            {
                camera.Move(-0.1f, 0f, 0f);
            }

            if (Keyboard.GetState().IsKeyDown(Key.D))
            {
                camera.Move(0.1f, 0f, 0f);
            }

            if (Keyboard.GetState().IsKeyDown(Key.Q))
            {
                camera.Move(0f, 0f, 0.1f);
            }

            if (Keyboard.GetState().IsKeyDown(Key.E))
            {
                camera.Move(0f, 0f, -0.1f);
            }

            if (Focused)
            {
                Vector2 delta = lastMousePos - new Vector2(OpenTK.Input.Mouse.GetState().X, OpenTK.Input.Mouse.GetState().Y);
                lastMousePos += delta;

                camera.AddRotation(delta.X, delta.Y);
                lastMousePos = new Vector2(OpenTK.Input.Mouse.GetState().X, OpenTK.Input.Mouse.GetState().Y);
            }
        }


        protected override void OnFocusedChanged(EventArgs e)
        {
            base.OnFocusedChanged(e);
            lastMousePos = new Vector2(OpenTK.Input.Mouse.GetState().X, OpenTK.Input.Mouse.GetState().Y);
        }


        int loadImage(Bitmap image)
        {
            int texID = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, texID);
            BitmapData data = image.LockBits(new System.Drawing.Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            image.UnlockBits(data);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            return texID;
        }

        int loadImage(string filename)
        {
            try
            {
                Bitmap file = new Bitmap(filename);
                return loadImage(file);
            }
            catch (FileNotFoundException)
            {
                return -1;
            }
        }
    }
}
