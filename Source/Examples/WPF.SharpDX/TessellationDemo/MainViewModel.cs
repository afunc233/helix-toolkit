﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainViewModel.cs" company="Helix Toolkit">
//   Copyright (c) 2014 Helix Toolkit contributors
// </copyright>
// <summary>
//   load the model from obj-file
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace TessellationDemo
{
    using System.Linq;
    using DemoCore;
    using HelixToolkit.Wpf.SharpDX;
    using SharpDX;
    using Media3D = System.Windows.Media.Media3D;
    using Point3D = System.Windows.Media.Media3D.Point3D;
    using Vector3D = System.Windows.Media.Media3D.Vector3D;
    using Transform3D = System.Windows.Media.Media3D.Transform3D;
    using System.Windows.Media.Imaging;
    using HelixToolkit.Wpf.SharpDX.Core;
    using System.IO;
    using SharpDX.Direct3D11;
    using System.Collections;
    using System.Collections.Generic;
    using static HelixToolkit.Wpf.SharpDX.Core.PatchMeshRenderCore;

    public class MainViewModel : BaseViewModel
    {
        public Geometry3D DefaultModel { get; private set; }
        public Geometry3D Lines { get; private set; }
        public Geometry3D Grid { get; private set; }

        public PhongMaterial DefaultMaterial { get; private set; }
        public SharpDX.Color GridColor { get; private set; }

        public Transform3D DefaultTransform { get; private set; }
        public Transform3D GridTransform { get; private set; }

        public Vector3 DirectionalLightDirection1 { get; private set; }
        public Vector3 DirectionalLightDirection2 { get; private set; }
        public Vector3 DirectionalLightDirection3 { get; private set; }
        public Color4 DirectionalLightColor { get; private set; }
        public Color4 AmbientLightColor { get; private set; }

        private FillMode fillMode = FillMode.Solid;
        public FillMode FillMode
        {
            set
            {
                SetValue(ref fillMode, value);
            }
            get
            {
                return fillMode;
            }
        }

        private bool wireFrame = false;
        public bool Wireframe
        {
            set
            {
                if(SetValue(ref wireFrame, value))
                {
                    if (value)
                    {
                        FillMode = FillMode.Wireframe;
                    }
                    else
                    {
                        FillMode = FillMode.Solid;
                    }
                }
            }
            get
            {
                return wireFrame;
            }
        }

        private MeshTopologyEnum meshTopology = MeshTopologyEnum.PNTriangles;

        public MeshTopologyEnum MeshTopology
        {
            get { return this.meshTopology; }
            set
            {
                /// if topology is changes, reload the model with proper type of faces
                this.meshTopology = value;
                this.LoadModel(@"./Media/teapot_quads_tex.obj", this.meshTopology == MeshTopologyEnum.PNTriangles ? 
                    MeshFaces.Default : MeshFaces.QuadPatches);
            }
        }

        private string renderTechniqueName = DefaultRenderTechniqueNames.Blinn;
        public string RenderTechniqueName
        {
            set
            {
                renderTechniqueName = value;
                RenderTechnique = EffectsManager[value];
            }
            get
            {
                return renderTechniqueName;
            }
        }

        public IList<Matrix> Instances { private set; get; }

        public MainViewModel()
        {
            EffectsManager = new DefaultShaderTechniqueManager();
            RenderTechnique = EffectsManager[DefaultRenderTechniqueNames.Blinn];
            // ----------------------------------------------
            // titles
            this.Title = "Hardware Tessellation Demo";
            this.SubTitle = "WPF & SharpDX";

            // ---------------------------------------------
            // camera setup
            this.Camera = new PerspectiveCamera { Position = new Point3D(7, 10, 12), LookDirection = new Vector3D(-7, -10, -12), UpDirection = new Vector3D(0, 1, 0) };

            // ---------------------------------------------
            // setup lighting            
            this.AmbientLightColor = new Color4(0.2f, 0.2f, 0.2f, 1.0f);
            this.DirectionalLightColor = Color.White;
            this.DirectionalLightDirection1 = new Vector3(-0, -50, -20);
            this.DirectionalLightDirection2 = new Vector3(-0, -1, +50);
            this.DirectionalLightDirection3 = new Vector3(0, +1, 0);

            // ---------------------------------------------
            // model trafo
            this.DefaultTransform = new Media3D.TranslateTransform3D(0, -0, 0);

            // ---------------------------------------------
            // model material
            this.DefaultMaterial = new PhongMaterial
            {
                AmbientColor = Color.Gray,
                DiffuseColor = new Color4(0.75f, 0.75f, 0.75f, 1.0f), // Colors.LightGray,
                SpecularColor = Color.White,
                SpecularShininess = 100f,
                DiffuseMap = LoadFileToMemory(new System.Uri(@"./Media/TextureCheckerboard2.dds", System.UriKind.RelativeOrAbsolute).ToString()),
                NormalMap = LoadFileToMemory(new System.Uri(@"./Media/TextureCheckerboard2_dot3.dds", System.UriKind.RelativeOrAbsolute).ToString()),
                DisplacementMap = LoadFileToMemory(new System.Uri(@"./Media/TextureCheckerboard2_dot3.dds", System.UriKind.RelativeOrAbsolute).ToString()),
            };

            // ---------------------------------------------
            // init model
            this.LoadModel(@"./Media/teapot_quads_tex.obj", this.meshTopology == MeshTopologyEnum.PNTriangles ?
             MeshFaces.Default : MeshFaces.QuadPatches);
            // ---------------------------------------------
            // floor plane grid
            this.Grid = LineBuilder.GenerateGrid(10);
            this.GridColor = SharpDX.Color.Black;
            this.GridTransform = new Media3D.TranslateTransform3D(-5, -4, -5);

            Instances = new Matrix[] { Matrix.Identity, Matrix.Translation(10, 0, 10), Matrix.Translation(-10, 0, 10), Matrix.Translation(10, 0, -10), Matrix.Translation(-10, 0, -10), };
        }

        /// <summary>
        /// load the model from obj-file
        /// </summary>
        /// <param name="filename">filename</param>
        /// <param name="faces">Determines if facades should be treated as triangles (Default) or as quads (Quads)</param>
        private void LoadModel(string filename, MeshFaces faces)
        {
            // load model
            var reader = new ObjReader();
            var objModel = reader.Read(filename, new ModelInfo() { Faces = faces });
            var model = objModel[0].Geometry as MeshGeometry3D;
            model.Colors = new Color4Collection(model.Positions.Select(x => new Color4(1, 0, 0, 1)));
            DefaultModel = model;
        }

    }
}