﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;

namespace FrameWork
{
    public static class StringImageConverter
    {
        //public static string ImageToBase64(Image image, System.Drawing.Imaging.ImageFormat format)
        //{
        //    try
        //    {
        //        using (MemoryStream ms = new MemoryStream())
        //        {
        //            // Convert Image to byte[]
        //            image.Save(ms, format);
        //            byte[] imageBytes = ms.ToArray();

        //            // Convert byte[] to Base64 String
        //            string base64String = Convert.ToBase64String(imageBytes);
        //            return base64String;
        //        }
        //    }
        //    catch (Exception exc)
        //    {
        //        return "error";
        //    }
        //}

        //public static Image Base64ToImage(string base64String)
        //{
        //    try
        //    {
        //        // Convert Base64 String to byte[]
        //        byte[] imageBytes = Convert.FromBase64String(base64String);
        //        MemoryStream ms = new MemoryStream(imageBytes, 0,
        //          imageBytes.Length);

        //        // Convert byte[] to Image
        //        ms.Write(imageBytes, 0, imageBytes.Length);
        //        Image image = Image.FromStream(ms, true);
        //        return image;
        //    }
        //    catch (Exception exc)
        //    {
        //        return null;
        //    }
        //}
    }
}