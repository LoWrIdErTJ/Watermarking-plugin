using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Windows.Forms;
using UBotPlugin;
using System.Linq;
using System.Windows;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Xml.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Security.Cryptography;
using System.Configuration;
using System.Media;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Net;
using System.Management;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Collections;
using System.Collections.Specialized;
using System.Data.SqlClient;
using Microsoft.VisualBasic;
using System.Reflection;
using System.Data.OleDb;

namespace CSVtoHTML
{

    // API KEY HERE
    public class PluginInfo
    {
        public static string HashCode { get { return "63020eed6a9faf4fe604994cebb545fe152c9317"; } }
    }

    // ---------------------------------------------------------------------------------------------------------- //
    //
    // ---------------------------------               COMMANDS               ----------------------------------- //
    //
    // ---------------------------------------------------------------------------------------------------------- //


    //
    //
    // WATERMARK WITH TEXT COLOR BY NAME
    //
    //
    public class WatermarkWithText : IUBotCommand
    {

        private List<UBotParameterDefinition> _parameters = new List<UBotParameterDefinition>();

        public WatermarkWithText()
        {
            _parameters.Add(new UBotParameterDefinition("Path to Image", UBotType.String));
            _parameters.Add(new UBotParameterDefinition("Path to Save to (.jpg)", UBotType.String));
            _parameters.Add(new UBotParameterDefinition("Watermark text", UBotType.String));

            var xParameter = new UBotParameterDefinition("Watermark font", UBotType.String);
            string familyName;
            string familyList = "";
            FontFamily[] fontFamilies;

            InstalledFontCollection installedFontCollection = new InstalledFontCollection();

            // Get the array of FontFamily objects.
            fontFamilies = installedFontCollection.Families;

            // The loop below creates a large string that is a comma-separated 
            // list of all font family names. 

            int count = fontFamilies.Length;
            for (int j = 0; j < count; ++j)
            {
                familyName = fontFamilies[j].Name;
                familyList = familyList + familyName;
                familyList = familyList + ",";
            }

            string[] pieces = familyList.Split(new string[] { "," }, StringSplitOptions.None);

            xParameter.Options = pieces;
            //xParameter.Options = new[] { "arial", "tahoma", "verdana" };
            _parameters.Add(xParameter);

            _parameters.Add(new UBotParameterDefinition("Text Size px", UBotType.String));

            var xColor = new UBotParameterDefinition("Font Color", UBotType.String);
            xColor.Options = new[] { "Black", "Blue", "Brown", "Green", "Orange", "Red", "White" };
            _parameters.Add(xColor);

           // _parameters.Add(new UBotParameterDefinition("Or Path to Watermark Image", UBotType.String));

            var Position = new UBotParameterDefinition("Position", UBotType.String);
            Position.Options = new[] { "", "Top Left", "Top Right", "Top Middle", "Bottom Left", "Bottom Right", "Bottom Middle", "Middle Left", "Middle Right", "Middle Center", "Custom XY" };
            _parameters.Add(Position);

            _parameters.Add(new UBotParameterDefinition("Custom X", UBotType.String));
            _parameters.Add(new UBotParameterDefinition("Custom Y", UBotType.String));
            _parameters.Add(new UBotParameterDefinition("Opacity 0-255", UBotType.String));
        }
       
        private static long[] ConvertStringArrayToLongArray(string str)
        {
            return str.Split(",".ToCharArray()).Select(x => long.Parse(x.ToString())).ToArray();
        }

        public string Category
        {
            get { return "File Commands"; }
        }

        public string CommandName
        {
            get { return "watermark with text"; }
        }


        public void Execute(IUBotStudio ubotStudio, Dictionary<string, string> parameters)
        {


            string pathToImage = parameters["Path to Image"];
            string saveToPath = parameters["Path to Save to (.jpg)"];
            
            string watermarkText = parameters["Watermark text"];
            //string watermarkImg = parameters["Or Path to Watermark Image"];

            string watermarkFont = parameters["Watermark font"];
            
            int textpx = Convert.ToInt32(parameters["Text Size px"]);
            
            string fontcol = parameters["Font Color"];
            int opac = Convert.ToInt32(parameters["Opacity 0-255"]);

            string position = parameters["Position"];

            string myx = parameters["Custom X"];
            string myy = parameters["Custom Y"];
            
            int cx = 0;
            int cy = 0;

            if (myx == "") { cx = Convert.ToInt32(0); } else { cx = Convert.ToInt32(parameters["Custom X"]); }
            if (myy == "") { cy = Convert.ToInt32(0); } else { cy = Convert.ToInt32(parameters["Custom Y"]); }

            
            Image image = Image.FromFile(pathToImage);
                
            try
            {
                Bitmap newImage = new Bitmap(image.Width, image.Height);
                

                using (Graphics gr = Graphics.FromImage(newImage))
                {
                 
                    
                    gr.SmoothingMode = SmoothingMode.AntiAlias;
                    gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    gr.DrawImage(image, new Rectangle(0, 0, newImage.Width, newImage.Height));

                    double diagonal = Math.Sqrt(newImage.Width * newImage.Width + newImage.Height * newImage.Height);

                    Rectangle containerBox = new Rectangle();

                        containerBox.X = (int)(diagonal / 10);
                        float messageLength = (float)(diagonal / watermarkText.Length * 1);
                        containerBox.Y = -(int)(messageLength / 1.6);

                        Font stringFont = new Font(watermarkFont, textpx);
                        Color mycolor = System.Drawing.Color.FromName(fontcol);

                        StringFormat sf = new StringFormat();

                        float slope = (float)(Math.Atan2(newImage.Height, newImage.Width) / Math.PI);// * 180 

                        gr.RotateTransform(slope);

                        if (position == "Top Left")
                        {
                            gr.DrawString(watermarkText, stringFont, new SolidBrush(Color.FromArgb(opac, mycolor)), 10, 10);
                        }
                        else if (position == "Top Right")
                        {
                            gr.DrawString(watermarkText, stringFont, new SolidBrush(Color.FromArgb(opac, mycolor)), newImage.Width - 10, 5);
                        }
                        else if (position == "Top Middle")
                        {
                            gr.DrawString(watermarkText, stringFont, new SolidBrush(Color.FromArgb(opac, mycolor)), (newImage.Width / 2), 10);
                        }
                        else if (position == "Bottom Left")
                        {
                            gr.DrawString(watermarkText, stringFont, new SolidBrush(Color.FromArgb(opac, mycolor)), 10, newImage.Height - 10);
                        }
                        else if (position == "Bottom Right")
                        {
                            gr.DrawString(watermarkText, stringFont, new SolidBrush(Color.FromArgb(opac, mycolor)), newImage.Width - 10, newImage.Height - 10);
                        }
                        else if (position == "Bottom Middle")
                        {
                            gr.DrawString(watermarkText, stringFont, new SolidBrush(Color.FromArgb(opac, mycolor)), (newImage.Width / 2), newImage.Height - 10);
                        }
                        else if (position == "Middle Left")
                        {
                            gr.DrawString(watermarkText, stringFont, new SolidBrush(Color.FromArgb(opac, mycolor)), 10, (newImage.Height / 2));
                        }
                        else if (position == "Middle Right")
                        {
                            gr.DrawString(watermarkText, stringFont, new SolidBrush(Color.FromArgb(opac, mycolor)), newImage.Width - 10, (newImage.Height / 2));
                        }
                        else if (position == "Middle Center")
                        {
                            gr.DrawString(watermarkText, stringFont, new SolidBrush(Color.FromArgb(opac, mycolor)), (newImage.Width / 2), (newImage.Height / 2));
                        }
                        else if (position == "Custom XY")
                        {
                            gr.DrawString(watermarkText, stringFont, new SolidBrush(Color.FromArgb(opac, mycolor)), cx, cy);
                        }
                        else
                        {
                            gr.DrawString(watermarkText, stringFont, new SolidBrush(Color.FromArgb(opac, mycolor)), (newImage.Width / 50), (newImage.Height / 50));
                        }



                        newImage.Save(saveToPath, System.Drawing.Imaging.ImageFormat.Jpeg);
                    
                   newImage.Dispose(); 
                } 
            }
            catch (Exception exc)
            {
                throw exc;
            }

            image.Dispose();
            
        }

        public int Calculations(decimal w1, decimal h1, int newWidth)
        {
            decimal height = 0;
            decimal ratio = 0;


            if (newWidth < w1)
            {
                ratio = w1 / newWidth;
                height = h1 / ratio;

                int i = Convert.ToInt32(height);
                return i;
            }

            if (w1 < newWidth)
            {
                ratio = newWidth / w1;
                height = h1 * ratio;

                int i = Convert.ToInt32(height);
                return i;
            }

            int c = Convert.ToInt32(height);
            return c;
        }

        public bool IsContainer
        {
            get { return false; }
        }

        public IEnumerable<UBotParameterDefinition> ParameterDefinitions
        {
            get { return _parameters; }
        }

        public UBotVersion UBotVersion
        {
            get { return UBotVersion.Standard; }
        }
    }


    //
    //
    // WATERMARK WITH TEXT COLOR BY HEX
    //
    //
    public class WatermarkWithTextColorHex : IUBotCommand
    {

        private List<UBotParameterDefinition> _parameters = new List<UBotParameterDefinition>();

        public WatermarkWithTextColorHex()
        {
            _parameters.Add(new UBotParameterDefinition("Path to Image", UBotType.String));
            _parameters.Add(new UBotParameterDefinition("Path to Save to (.jpg)", UBotType.String));
            _parameters.Add(new UBotParameterDefinition("Watermark text", UBotType.String));

            var xParameter = new UBotParameterDefinition("Watermark font", UBotType.String);
            string familyName;
            string familyList = "";
            FontFamily[] fontFamilies;

            InstalledFontCollection installedFontCollection = new InstalledFontCollection();

            // Get the array of FontFamily objects.
            fontFamilies = installedFontCollection.Families;

            // The loop below creates a large string that is a comma-separated 
            // list of all font family names. 

            int count = fontFamilies.Length;
            for (int j = 0; j < count; ++j)
            {
                familyName = fontFamilies[j].Name;
                familyList = familyList + familyName;
                familyList = familyList + ",";
            }

            string[] pieces = familyList.Split(new string[] { "," }, StringSplitOptions.None);

            xParameter.Options = pieces;
            //xParameter.Options = new[] { "arial", "tahoma", "verdana" };
            _parameters.Add(xParameter);

            _parameters.Add(new UBotParameterDefinition("Text Size px", UBotType.String));

            _parameters.Add(new UBotParameterDefinition("Font Color Hex", UBotType.String)); 
            
            // _parameters.Add(new UBotParameterDefinition("Or Path to Watermark Image", UBotType.String));

            var Position = new UBotParameterDefinition("Position", UBotType.String);
            Position.Options = new[] { "", "Top Left", "Top Right", "Top Middle", "Bottom Left", "Bottom Right", "Bottom Middle", "Middle Left", "Middle Right", "Middle Center", "Custom XY" };
            _parameters.Add(Position);

            _parameters.Add(new UBotParameterDefinition("Custom X", UBotType.String));
            _parameters.Add(new UBotParameterDefinition("Custom Y", UBotType.String));
            _parameters.Add(new UBotParameterDefinition("Opacity 0-255", UBotType.String));
        }

        private static long[] ConvertStringArrayToLongArray(string str)
        {
            return str.Split(",".ToCharArray()).Select(x => long.Parse(x.ToString())).ToArray();
        }

        public string Category
        {
            get { return "File Commands"; }
        }

        public string CommandName
        {
            get { return "watermark with text color hex"; }
        }


        public void Execute(IUBotStudio ubotStudio, Dictionary<string, string> parameters)
        {


            string pathToImage = parameters["Path to Image"];
            string saveToPath = parameters["Path to Save to (.jpg)"];

            string watermarkText = parameters["Watermark text"];
            //string watermarkImg = parameters["Or Path to Watermark Image"];

            string watermarkFont = parameters["Watermark font"];

            int textpx = Convert.ToInt32(parameters["Text Size px"]);

            string fontcol = parameters["Font Color Hex"];
            int opac = Convert.ToInt32(parameters["Opacity 0-255"]);

            string position = parameters["Position"];

            string myx = parameters["Custom X"];
            string myy = parameters["Custom Y"];

            int cx = 0;
            int cy = 0;

            if (myx == "") { cx = Convert.ToInt32(0); } else { cx = Convert.ToInt32(parameters["Custom X"]); }
            if (myy == "") { cy = Convert.ToInt32(0); } else { cy = Convert.ToInt32(parameters["Custom Y"]); }


            Image image = Image.FromFile(pathToImage);

            try
            {
                Bitmap newImage = new Bitmap(image.Width, image.Height);


                using (Graphics gr = Graphics.FromImage(newImage))
                {


                    gr.SmoothingMode = SmoothingMode.AntiAlias;
                    gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    gr.DrawImage(image, new Rectangle(0, 0, newImage.Width, newImage.Height));

                    double diagonal = Math.Sqrt(newImage.Width * newImage.Width + newImage.Height * newImage.Height);

                    Rectangle containerBox = new Rectangle();

                    containerBox.X = (int)(diagonal / 10);
                    float messageLength = (float)(diagonal / watermarkText.Length * 1);
                    containerBox.Y = -(int)(messageLength / 1.6);

                    Font stringFont = new Font(watermarkFont, textpx);

                    string hex = fontcol;
                    Color mycolor = System.Drawing.ColorTranslator.FromHtml(hex); 
                    
                    StringFormat sf = new StringFormat();

                    float slope = (float)(Math.Atan2(newImage.Height, newImage.Width) / Math.PI);// * 180 

                    gr.RotateTransform(slope);

                    if (position == "Top Left")
                    {
                        gr.DrawString(watermarkText, stringFont, new SolidBrush(Color.FromArgb(opac, mycolor)), 10, 10);
                    }
                    else if (position == "Top Right")
                    {
                        gr.DrawString(watermarkText, stringFont, new SolidBrush(Color.FromArgb(opac, mycolor)), newImage.Width - 10, 5);
                    }
                    else if (position == "Top Middle")
                    {
                        gr.DrawString(watermarkText, stringFont, new SolidBrush(Color.FromArgb(opac, mycolor)), (newImage.Width / 2), 10);
                    }
                    else if (position == "Bottom Left")
                    {
                        gr.DrawString(watermarkText, stringFont, new SolidBrush(Color.FromArgb(opac, mycolor)), 10, newImage.Height - 10);
                    }
                    else if (position == "Bottom Right")
                    {
                        gr.DrawString(watermarkText, stringFont, new SolidBrush(Color.FromArgb(opac, mycolor)), newImage.Width - 10, newImage.Height - 10);
                    }
                    else if (position == "Bottom Middle")
                    {
                        gr.DrawString(watermarkText, stringFont, new SolidBrush(Color.FromArgb(opac, mycolor)), (newImage.Width / 2), newImage.Height - 10);
                    }
                    else if (position == "Middle Left")
                    {
                        gr.DrawString(watermarkText, stringFont, new SolidBrush(Color.FromArgb(opac, mycolor)), 10, (newImage.Height / 2));
                    }
                    else if (position == "Middle Right")
                    {
                        gr.DrawString(watermarkText, stringFont, new SolidBrush(Color.FromArgb(opac, mycolor)), newImage.Width - 10, (newImage.Height / 2));
                    }
                    else if (position == "Middle Center")
                    {
                        gr.DrawString(watermarkText, stringFont, new SolidBrush(Color.FromArgb(opac, mycolor)), (newImage.Width / 2), (newImage.Height / 2));
                    }
                    else if (position == "Custom XY")
                    {
                        gr.DrawString(watermarkText, stringFont, new SolidBrush(Color.FromArgb(opac, mycolor)), cx, cy);
                    }
                    else
                    {
                        gr.DrawString(watermarkText, stringFont, new SolidBrush(Color.FromArgb(opac, mycolor)), (newImage.Width / 50), (newImage.Height / 50));
                    }



                    newImage.Save(saveToPath, System.Drawing.Imaging.ImageFormat.Jpeg);

                    newImage.Dispose();
                }
            }
            catch (Exception exc)
            {
                throw exc;
            }

            image.Dispose();

        }

        public int Calculations(decimal w1, decimal h1, int newWidth)
        {
            decimal height = 0;
            decimal ratio = 0;


            if (newWidth < w1)
            {
                ratio = w1 / newWidth;
                height = h1 / ratio;

                int i = Convert.ToInt32(height);
                return i;
            }

            if (w1 < newWidth)
            {
                ratio = newWidth / w1;
                height = h1 * ratio;

                int i = Convert.ToInt32(height);
                return i;
            }

            int c = Convert.ToInt32(height);
            return c;
        }

        public bool IsContainer
        {
            get { return false; }
        }

        public IEnumerable<UBotParameterDefinition> ParameterDefinitions
        {
            get { return _parameters; }
        }

        public UBotVersion UBotVersion
        {
            get { return UBotVersion.Standard; }
        }
    }


    //
    //
    // WATERMARK WITH IMAGE
    //
    //
    public class WatermarkWithImage : IUBotCommand
    {

        private List<UBotParameterDefinition> _parameters = new List<UBotParameterDefinition>();

        public WatermarkWithImage()
        {
            _parameters.Add(new UBotParameterDefinition("Path to Original Image", UBotType.String));
            _parameters.Add(new UBotParameterDefinition("Path to Logo Image", UBotType.String));
            _parameters.Add(new UBotParameterDefinition("Save to path (.jpg)", UBotType.String));

            var Position = new UBotParameterDefinition("Position", UBotType.String);
            Position.Options = new[] { "", "Top Left", "Top Right", "Top Middle", "Bottom Left", "Bottom Right", "Bottom Middle", "Middle Left", "Middle Right", "Middle Center", "Custom XY" };
            _parameters.Add(Position);

            _parameters.Add(new UBotParameterDefinition("Custom X", UBotType.String));
            _parameters.Add(new UBotParameterDefinition("Custom Y", UBotType.String));
        }

        public string Category
        {
            get { return "File Commands"; }
        }

        public string CommandName
        {
            get { return "watermark with image"; }
        }


        public void Execute(IUBotStudio ubotStudio, Dictionary<string, string> parameters)
        {


            string pathToImage = parameters["Path to Original Image"];
            string pathToLogo = parameters["Path to Logo Image"];
            string saveToPath = parameters["Save to path (.jpg)"];

            string position = parameters["Position"];

            string myx = parameters["Custom X"];
            string myy = parameters["Custom Y"];

            int cx = 0;
            int cy = 0;

            if (myx == "") { cx = Convert.ToInt32(0); } else { cx = Convert.ToInt32(parameters["Custom X"]); }
            if (myy == "") { cy = Convert.ToInt32(0); } else { cy = Convert.ToInt32(parameters["Custom Y"]); }


            using (Image image = Image.FromFile(pathToImage))
            using (Image watermarkImage = Image.FromFile(pathToLogo))
            using (Graphics imageGraphics = Graphics.FromImage(image))
            using (TextureBrush watermarkBrush = new TextureBrush(watermarkImage))
            {

                if (position == "Top Left")
                {
                    int x = 10;
                    int y = 10;
                    watermarkBrush.TranslateTransform(x, y);
                    imageGraphics.FillRectangle(watermarkBrush, new System.Drawing.Rectangle(new System.Drawing.Point(x, y), new System.Drawing.Size(watermarkImage.Width + 1, watermarkImage.Height)));
                }
                else if (position == "Top Right")
                {
                    int x = (image.Width - watermarkImage.Width - 5);
                    int y = 10;
                    watermarkBrush.TranslateTransform(x, y);
                    imageGraphics.FillRectangle(watermarkBrush, new System.Drawing.Rectangle(new System.Drawing.Point(x, y), new System.Drawing.Size(watermarkImage.Width + 1, watermarkImage.Height)));
                }
                else if (position == "Top Middle")
                {
                    int x = (image.Width / 2 - watermarkImage.Width / 2);
                    int y = 10;
                    watermarkBrush.TranslateTransform(x, y);
                    imageGraphics.FillRectangle(watermarkBrush, new System.Drawing.Rectangle(new System.Drawing.Point(x, y), new System.Drawing.Size(watermarkImage.Width + 1, watermarkImage.Height)));
                }
                else if (position == "Bottom Left")
                {
                    int x = 10;
                    int y = (image.Height - 10 - watermarkImage.Width);
                    watermarkBrush.TranslateTransform(x, y);
                    imageGraphics.FillRectangle(watermarkBrush, new System.Drawing.Rectangle(new System.Drawing.Point(x, y), new System.Drawing.Size(watermarkImage.Width + 1, watermarkImage.Height)));
                }
                else if (position == "Bottom Right")
                {
                    int x = (image.Width - watermarkImage.Width - 5);
                    int y = (image.Height - 10 - watermarkImage.Width);
                    watermarkBrush.TranslateTransform(x, y);
                    imageGraphics.FillRectangle(watermarkBrush, new System.Drawing.Rectangle(new System.Drawing.Point(x, y), new System.Drawing.Size(watermarkImage.Width + 1, watermarkImage.Height)));
                }
                else if (position == "Bottom Middle")
                {
                    int x = (image.Width / 2 - watermarkImage.Width / 2);
                    int y = (image.Height - 10 - watermarkImage.Width);
                    watermarkBrush.TranslateTransform(x, y);
                    imageGraphics.FillRectangle(watermarkBrush, new System.Drawing.Rectangle(new System.Drawing.Point(x, y), new System.Drawing.Size(watermarkImage.Width + 1, watermarkImage.Height)));
                }
                else if (position == "Middle Left")
                {
                    int x = 10;
                    int y = (image.Height / 2 - watermarkImage.Height / 2);
                    watermarkBrush.TranslateTransform(x, y);
                    imageGraphics.FillRectangle(watermarkBrush, new System.Drawing.Rectangle(new System.Drawing.Point(x, y), new System.Drawing.Size(watermarkImage.Width + 1, watermarkImage.Height)));
                }
                else if (position == "Middle Right")
                {
                    int x = (image.Width - watermarkImage.Width - 5);
                    int y = (image.Height / 2 - watermarkImage.Height / 2);
                    watermarkBrush.TranslateTransform(x, y);
                    imageGraphics.FillRectangle(watermarkBrush, new System.Drawing.Rectangle(new System.Drawing.Point(x, y), new System.Drawing.Size(watermarkImage.Width + 1, watermarkImage.Height)));
                }
                else if (position == "Middle Center")
                {
                    int x = (image.Width / 2 - watermarkImage.Width / 2);
                    int y = (image.Height / 2 - watermarkImage.Height / 2);
                    watermarkBrush.TranslateTransform(x, y);
                    imageGraphics.FillRectangle(watermarkBrush, new System.Drawing.Rectangle(new System.Drawing.Point(x, y), new System.Drawing.Size(watermarkImage.Width + 1, watermarkImage.Height)));
                }
                else if (position == "Custom XY")
                {
                    int x = cx;
                    int y = cy;
                    watermarkBrush.TranslateTransform(x, y);
                    imageGraphics.FillRectangle(watermarkBrush, new System.Drawing.Rectangle(new System.Drawing.Point(x, y), new System.Drawing.Size(watermarkImage.Width + 1, watermarkImage.Height)));
                }
                else
                {
                    int x = (image.Width / 2 - watermarkImage.Width / 2);
                    int y = (image.Height / 2 - watermarkImage.Height / 2);
                    watermarkBrush.TranslateTransform(x, y);
                    imageGraphics.FillRectangle(watermarkBrush, new System.Drawing.Rectangle(new System.Drawing.Point(x, y), new System.Drawing.Size(watermarkImage.Width + 1, watermarkImage.Height)));
                }

                image.Save(saveToPath);

                image.Dispose();
                watermarkImage.Dispose();
                imageGraphics.Dispose();
                watermarkBrush.Dispose();

            }
        }

        public bool IsContainer
        {
            get { return false; }
        }

        public IEnumerable<UBotParameterDefinition> ParameterDefinitions
        {
            get { return _parameters; }
        }

        public UBotVersion UBotVersion
        {
            get { return UBotVersion.Standard; }
        }
    }



    //
    //
    // RESIZE IMAGE
    //
    //
    public class ResizeImg : IUBotCommand
    {

        private List<UBotParameterDefinition> _parameters = new List<UBotParameterDefinition>();

        public ResizeImg()
        {
            _parameters.Add(new UBotParameterDefinition("Path to Image", UBotType.String));
            _parameters.Add(new UBotParameterDefinition("Save Image to", UBotType.String));
            _parameters.Add(new UBotParameterDefinition("New Width", UBotType.String));
            _parameters.Add(new UBotParameterDefinition("New Height", UBotType.String));
            var xParameter = new UBotParameterDefinition("Preserve Aspect ratio?", UBotType.String);
            xParameter.Options = new[] { "", "Yes", "No" };
            _parameters.Add(xParameter);
        }

        public string Category
        {
            get { return "File Commands"; }
        }

        public string CommandName
        {
            get { return "resize image"; }
        }


        public void Execute(IUBotStudio ubotStudio, Dictionary<string, string> parameters)
        {


            string pathToImage = parameters["Path to Image"];
            string savetopath = parameters["Save Image to"];
            int nwidth = Convert.ToInt32(parameters["New Width"]);
            int nheight = Convert.ToInt32(parameters["New Height"]);
            string AspectRatio = parameters["Preserve Aspect ratio?"];


            Image original = Image.FromFile(pathToImage);

            if (AspectRatio == "Yes")
            {
                Image resized = ResizeImage(original, new System.Drawing.Size(nwidth, nheight));
                resized.Save(savetopath, ImageFormat.Png);
                original.Dispose();
                resized.Dispose();
            }
            else if (AspectRatio == "No")
            {
                Image resized = ResizeImagef(original, new System.Drawing.Size(nwidth, nheight));
                resized.Save(savetopath, ImageFormat.Png);
                original.Dispose();
                resized.Dispose();
            }
            else
            {
                Image resized = ResizeImagef(original, new System.Drawing.Size(nwidth, nheight));
                resized.Save(savetopath, ImageFormat.Png);
                original.Dispose();
                resized.Dispose();
            }

        }

        public static Image ResizeImage(Image image, System.Drawing.Size size, bool preserveAspectRatio = true)
        {
            int newWidth;
            int newHeight;
            if (preserveAspectRatio)
            {
                int originalWidth = image.Width;
                int originalHeight = image.Height;
                float percentWidth = (float)size.Width / (float)originalWidth;
                float percentHeight = (float)size.Height / (float)originalHeight;
                float percent = percentHeight < percentWidth ? percentHeight : percentWidth;
                newWidth = (int)(originalWidth * percent);
                newHeight = (int)(originalHeight * percent);
            }
            else
            {
                newWidth = size.Width;
                newHeight = size.Height;
            }
            Image newImage = new Bitmap(newWidth, newHeight);
            using (Graphics graphicsHandle = Graphics.FromImage(newImage))
            {
                graphicsHandle.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphicsHandle.DrawImage(image, 0, 0, newWidth, newHeight);
            }
            return newImage;
        }

        public static Image ResizeImagef(Image image, System.Drawing.Size size, bool preserveAspectRatio = false)
        {
            int newWidth;
            int newHeight;

            newWidth = size.Width;
            newHeight = size.Height;

            Image newImage = new Bitmap(newWidth, newHeight);
            using (Graphics graphicsHandle = Graphics.FromImage(newImage))
            {
                graphicsHandle.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphicsHandle.DrawImage(image, 0, 0, newWidth, newHeight);
            }
            return newImage;
        }

        public bool IsContainer
        {
            get { return false; }
        }

        public IEnumerable<UBotParameterDefinition> ParameterDefinitions
        {
            get { return _parameters; }
        }

        public UBotVersion UBotVersion
        {
            get { return UBotVersion.Standard; }
        }

    }
    

}
