using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using OpenTK;
using SkiaSharp;
using Point = System.Drawing.Point;

namespace TransUI
{
    public class MainWindow : Window
    {
        private Game game;
        private Scene scene;

        ComboBox objectComboBox;
        ComboBox faceComboBox;
        ComboBox modeComboBox;

        Slider XSlider;
        Slider YSlider;
        Slider ZSlider;

        ToggleSwitch TextureSwitch;

        private float minRotate = -180f;
        private float maxRotate = 180f;

        private float minTraslate = -5f;
        private float maxTraslate = 5f;

        private float minScale = 0f;
        private float maxScale = 2f;

        public MainWindow()
        {
            InitializeComponent();


            Opened += OnInitialized;

            objectComboBox = this.Find<ComboBox>("ObjectComboBox");
            faceComboBox = this.Find<ComboBox>("FaceComboBox");
            modeComboBox = this.Find<ComboBox>("ModeComboBox");

            XSlider = this.Find<Slider>("XSlider");
            YSlider = this.Find<Slider>("YSlider");
            ZSlider = this.Find<Slider>("ZSlider");

            TextureSwitch = this.Find<ToggleSwitch>("TexutreSwitch");


            scene = new Scene(new Vertex(0f, 0f, 0f));
            scene.Add("cubo", Object3D.LoadFromJson("../../../object/Casa.json"));
            scene.Add("techo", Object3D.LoadFromJson("../../../object/Techo.json"));
            scene.Add("cono", Object3D.LoadFromJson("../../../object/Cono.json"));

            objectComboBox.Items = scene.GetObjects().Keys.Prepend("Escenario");
            objectComboBox.SelectedIndex = 0;

            modeComboBox.Items = new List<string> {"Rotación", "Traslación", "Escalado"};
            modeComboBox.SelectedIndex = 0;

            updateFaceItems();

            Thread openGL = new(openGLHandler);
            openGL.Start();


#if DEBUG
            this.AttachDevTools();
#endif
        }

        protected override void OnClosed(EventArgs e)
        {
            game.Close();
            base.OnClosed(e);
        }


        private void openGLHandler(object? obj)
        {
            game = new Game(800, 800, "Tee");
            game.Location = new Point(1000, 0);

            game.scene = scene;

            game.UpdateFrame += onUpdateFrameHandler;
            game.Run(100);
        }

        private void onUpdateFrameHandler(object? sender, FrameEventArgs e)
        {
        }

        private void OnInitialized(object? sender, EventArgs e)
        {
            Position = new PixelPoint(200, 0);
            Width = 400;
            Height = 600;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            Position = new PixelPoint(0, 0);
        }

        private void ObjectSelected(object? sender, SelectionChangedEventArgs e)
        {
            updateFaceItems();
        }

        private void updateFaceItems()

        {
            string key = (string) objectComboBox.SelectedItem;
            if (key == "Escenario")
            {
                faceComboBox.IsEnabled = false;
                faceComboBox.Items = null;
            }
            else
            {
                faceComboBox.IsEnabled = true;
                faceComboBox.Items = scene.GetObject3D(key).getListOfFaces().Keys.Prepend("Objeto");
                faceComboBox.SelectedIndex = 0;
            }
        }

        private void SliderHandler(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (XSlider != null && YSlider != null && ZSlider != null)
            {
                string mode = (string) modeComboBox.SelectedItem;
                string objectString = (string) objectComboBox.SelectedItem;
                string faceString = (string) faceComboBox.SelectedItem;


                if (mode == "Rotación")
                {
                    if (objectString == "Escenario")
                    {
                        game.scene.SetRotation((float) XSlider.Value, (float) YSlider.Value,
                            (float) ZSlider.Value);
                    }
                    else
                    {
                        Object3D objectToProcess = game.scene.GetObject3D(objectString);
                        if (faceString == "Objeto")
                        {
                            objectToProcess.SetRotation((float) XSlider.Value, (float) YSlider.Value,
                                (float) ZSlider.Value);
                            return;
                        }

                        Face faceToProcess = objectToProcess.GetFace(faceString);
                        faceToProcess.SetRotation((float) XSlider.Value, (float) YSlider.Value,
                            (float) ZSlider.Value);
                    }

                    return;
                }

                Vertex coordinates = new Vertex((float) XSlider.Value, (float) YSlider.Value,
                    (float) ZSlider.Value);

                if (mode == "Traslación")
                {
                    if (objectString == "Escenario")
                    {
                        game.scene.SetTraslation(coordinates);
                    }
                    else
                    {
                        Object3D objectToProcess = game.scene.GetObject3D(objectString);
                        if (faceString == "Objeto")
                        {
                            objectToProcess.SetTraslation(coordinates);
                            return;
                        }

                        Face faceToProcess = objectToProcess.GetFace(faceString);
                        faceToProcess.SetTraslation(coordinates);
                    }

                    return;
                }

                if (mode == "Escalado")
                {
                    if (objectString == "Escenario")
                    {
                        game.scene.SetScale(coordinates);
                    }
                    else
                    {
                        Object3D objectToProcess = game.scene.GetObject3D(objectString);
                        if (faceString == "Objeto")
                        {
                            objectToProcess.SetScale(coordinates);
                            return;
                        }

                        Face faceToProcess = objectToProcess.GetFace(faceString);
                        faceToProcess.SetScale(coordinates);
                    }
                }
            }
        }

        private void ModeSelected(object? sender, SelectionChangedEventArgs e)
        {
            switch (modeComboBox.SelectedItem)
            {
                case "Rotación":
                    setSlidersRange(minRotate, maxRotate);
                    break;
                case "Traslación":
                    setSlidersRange(minTraslate, maxTraslate);
                    break;
                case "Escalado":
                    setSlidersRange(minScale, maxScale);
                    break;
            }
        }

        private void setSlidersRange(float minValue, float maxValue)
        {
            XSlider.Minimum = minValue;
            XSlider.Maximum = maxValue;
            YSlider.Minimum = minValue;
            YSlider.Maximum = maxValue;
            ZSlider.Minimum = minValue;
            ZSlider.Maximum = maxValue;

            if (modeComboBox.SelectedItem == "Escalado")
            {
                XSlider.Value = 1f;
                YSlider.Value = 1f;
                ZSlider.Value = 1f;
                return;
            }

            XSlider.Value = 0;
            YSlider.Value = 0;
            ZSlider.Value = 0;
        }

        private void SwitchHandler(object? sender, RoutedEventArgs e)
        {
            game.scene.SetTextureType((bool) TextureSwitch.IsChecked ? 9 : 2);
        }
    }
}