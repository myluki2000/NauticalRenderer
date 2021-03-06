﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Ionic.Zip;
using Microsoft.Xna.Framework;
using NauticalRenderer.Utility;
using OsmSharp.Complete;
using OsmSharp.Streams;
using OsmSharp.Streams.Complete;

namespace NauticalRenderer.Data.MapPack
{
    public class MapPack
    {
        private Vector2[] boundingPoly = null;
        public Vector2[] BoundingPolygon
        {
            get
            {
                if (boundingPoly != null) return boundingPoly;

                LoadBoundingPoly(OpenFile("boundary.poly"));
                return boundingPoly;
            }
        }
        private readonly string zipPath;

        /// <inheritdoc />
        public MapPack(string zipPath)
        {
            this.zipPath = zipPath;
        }

        public Stream OpenFile(string mapName)
        {
            using var ms = new MemoryStream();
            Globals.ResourceManager.GetStreamForFile(zipPath).CopyTo(ms);
            ms.Position = 0;

            Stream output = new MemoryStream();
            using (ZipFile zip = ZipFile.Read(ms))
            {
                ZipEntry entry = zip[mapName];
                entry.Extract(output);
            }

            output.Seek(0, SeekOrigin.Begin);
            return output;
        }


        public void LoadBoundingPoly(Stream boundingPolyStream)
        {
            List<Vector2> coords = new List<Vector2>();
            using (StreamReader sr = new StreamReader(boundingPolyStream))
            {
                // skip first two lines
                sr.ReadLine();
                sr.ReadLine();

                string line = sr.ReadLine();
                while (line != "END")
                {
                    line = line.Trim();
                    line = line.Replace("   ", " ");
                    string[] coordsString = line.Split(' ');

                    coords.Add(new Vector2(float.Parse(coordsString[0], CultureInfo.InvariantCulture),
                            -float.Parse(coordsString[1], CultureInfo.InvariantCulture)));

                    line = sr.ReadLine();
                }
            }

            boundingPoly = coords.ToArray();
        }


        public MapData Data;

        public class MapData
        {

        }
    }
}
