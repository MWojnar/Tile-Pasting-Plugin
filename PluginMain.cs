using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Drawing;
using Community.CsharpSqlite.SQLiteClient;
using MySql.Data.MySqlClient;
using Microsoft.Xna.Framework;
using Terraria;
using TerrariaAPI;
using TerrariaAPI.Hooks;
using TShockAPI;
using TShockAPI.DB;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Threading;

namespace PluginTemplate
{
    [APIVersion(1, 8)]
    public class PluginTemplate : TerrariaPlugin
    {
        public static Dictionary<string, byte> tileTypeNames = new Dictionary<string, byte>();
        public static Dictionary<string, byte> wallTypeNames = new Dictionary<string, byte>();
        public static double[] brushStroke = new double[256];
        public static int[] copyX = new int[256];
        public static int[] copyY = new int[256];
        public static int[] copyW = new int[256];
        public static int[] copyH = new int[256];
        public static int[] clipboardType = new int[256];
        public static bool[] cut = new bool[256];
        public static TileCollection[] undoTiles = new TileCollection[256];
        public static System.Drawing.Point[] undoPoint1 = new System.Drawing.Point[256];
        public static System.Drawing.Point[] undoPoint2 = new System.Drawing.Point[256];
        public static System.Drawing.Point[] lastArea1 = new System.Drawing.Point[256];
        public static System.Drawing.Point[] lastArea2 = new System.Drawing.Point[256];
        public override string Name
        {
            get { return "TileEditing"; }
        }
        public override string Author
        {
            get { return "Created by DaGamesta"; }
        }
        public override string Description
        {
            get { return "Mold the world around you with ease!"; }
        }
        public override Version Version
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version; }
        }

        public override void Initialize()
        {
            GameHooks.Initialize += OnInitialize;
            ServerHooks.Leave += OnLeave;
        }
        public override void DeInitialize()
        {
            GameHooks.Initialize -= OnInitialize;
            ServerHooks.Leave -= OnLeave;
        }
        public PluginTemplate(Main game)
            : base(game)
        {
            Order = -2;
        }

        public void OnInitialize()
        {
            for (int i = 0; i < 256; i++)
            {

                copyX[i] = 0;
                copyY[i] = 0;
                copyW[i] = 0;
                copyH[i] = 0;
                clipboardType[i] = 0;
                brushStroke[i] = 1.0;
                lastArea1[i] = System.Drawing.Point.Empty;
                lastArea2[i] = System.Drawing.Point.Empty;
                undoPoint1[i] = System.Drawing.Point.Empty;
                undoPoint2[i] = System.Drawing.Point.Empty;
                cut[i] = false;

            }
            bool tilepaste = false;
            tileTypeNames.Add("dirt",0);
            tileTypeNames.Add("stone",1);
            tileTypeNames.Add("grass",2);
            tileTypeNames.Add("iron ore",6);
            tileTypeNames.Add("copper ore",7);
            tileTypeNames.Add("gold ore",8);
            tileTypeNames.Add("silver ore",9);
            tileTypeNames.Add("wooden platform",19);
            tileTypeNames.Add("demonite ore",22);
            tileTypeNames.Add("corrupted grass",23);
            tileTypeNames.Add("ebonstone",25);
            tileTypeNames.Add("wood",30);
            tileTypeNames.Add("corruption thorn",32);
            tileTypeNames.Add("meteorite",37);
            tileTypeNames.Add("gray brick",38);
            tileTypeNames.Add("clay brick",39);
            tileTypeNames.Add("clay",40);
            tileTypeNames.Add("blue brick",41);
            tileTypeNames.Add("green brick",43);
            tileTypeNames.Add("pink brick",44);
            tileTypeNames.Add("gold brick",45);
            tileTypeNames.Add("silver brick",46);
            tileTypeNames.Add("copper brick",47);
            tileTypeNames.Add("spike",48);
            tileTypeNames.Add("cobweb",51);
            tileTypeNames.Add("sand",53);
            tileTypeNames.Add("glass",54);
            tileTypeNames.Add("obsidian",56);
            tileTypeNames.Add("ash",57);
            tileTypeNames.Add("hellstone",58);
            tileTypeNames.Add("mud",59);
            tileTypeNames.Add("jungle grass",60);
            tileTypeNames.Add("sapphire",63);
            tileTypeNames.Add("ruby",64);
            tileTypeNames.Add("emerald",65);
            tileTypeNames.Add("topaz",66);
            tileTypeNames.Add("amethyst",67);
            tileTypeNames.Add("diamond", 68);
            tileTypeNames.Add("mushroom grass", 70);
            tileTypeNames.Add("obsidian brick",75);
            tileTypeNames.Add("hellstone brick", 76);
            tileTypeNames.Add("water", 253);
            tileTypeNames.Add("lava", 254);
            wallTypeNames.Add("stone wall",1);
            wallTypeNames.Add("dirt wall untakeable",2);
            wallTypeNames.Add("ebonstone wall",3);
            wallTypeNames.Add("wood wall", 4);
            wallTypeNames.Add("gray brick wall", 5);
            wallTypeNames.Add("red brick wall", 6);
            wallTypeNames.Add("blue brick wall", 7);
            wallTypeNames.Add("green brick wall", 8);
            wallTypeNames.Add("pink brick wall", 9);
            wallTypeNames.Add("gold brick wall", 10);
            wallTypeNames.Add("silver brick wall", 11);
            wallTypeNames.Add("copper brick wall", 12);
            wallTypeNames.Add("hellstone brick wall", 13);
            wallTypeNames.Add("obsidian brick wall", 14);
            wallTypeNames.Add("mud wall", 15);
            wallTypeNames.Add("dirt wall", 16);

            foreach (Group group in TShock.Groups.groups)
            {
                if (group.Name != "superadmin")
                {
                    if (group.HasPermission("tilepasting"))
                        tilepaste = true;
                }
            }
            List<string> permlist = new List<string>();
            if (!tilepaste)
                permlist.Add("tilepasting");
            TShock.Groups.AddPermissions("trustedadmin", permlist);
            permlist = new List<string>();

            Commands.ChatCommands.Add(new Command("tilepasting", Rectangle, "rectangle"));
            Commands.ChatCommands.Add(new Command("tilepasting", RectangleOutline, "rectangleoutline"));
            Commands.ChatCommands.Add(new Command("tilepasting", Oval, "oval"));
            Commands.ChatCommands.Add(new Command("tilepasting", OvalOutline, "ovaloutline"));
            Commands.ChatCommands.Add(new Command("tilepasting", SemiOval, "semioval"));
            Commands.ChatCommands.Add(new Command("tilepasting", SemiOvalOutline, "semiovaloutline"));
            Commands.ChatCommands.Add(new Command("tilepasting", Circle, "circle"));
            Commands.ChatCommands.Add(new Command("tilepasting", CircleOutline, "circleoutline"));
            Commands.ChatCommands.Add(new Command("tilepasting", SemiCircle, "semicircle"));
            Commands.ChatCommands.Add(new Command("tilepasting", SemiCircleOutline, "semicircleoutline"));
            Commands.ChatCommands.Add(new Command("tilepasting", ClearArea, "cleararea"));
            Commands.ChatCommands.Add(new Command("tilepasting", ClearArea, "clear"));
            Commands.ChatCommands.Add(new Command("tilepasting", Cut, "cut"));
            Commands.ChatCommands.Add(new Command("tilepasting", Copy, "copy"));
            Commands.ChatCommands.Add(new Command("tilepasting", Paste, "paste"));
            Commands.ChatCommands.Add(new Command("tilepasting", Replace, "replace"));
            Commands.ChatCommands.Add(new Command("tilepasting", Line, "line"));
            Commands.ChatCommands.Add(new Command("tilepasting", BrushStroke, "brushstroke"));
            Commands.ChatCommands.Add(new Command("tilepasting", LastArea, "lastarea"));
            Commands.ChatCommands.Add(new Command("tilepasting", Undo, "undo"));
        }

        private static void OnLeave(int ply)
        {

            copyX[ply] = 0;
            copyY[ply] = 0;
            copyW[ply] = 0;
            copyH[ply] = 0;
            cut[ply] = false;
            brushStroke[ply] = 1.0;
            lastArea1[ply] = System.Drawing.Point.Empty;
            lastArea2[ply] = System.Drawing.Point.Empty;

        }

        public static void Undo(CommandArgs args)
        {

            if (undoPoint1[args.Player.Index] != PointF.Empty)
            {

                int width = Math.Abs(undoPoint1[args.Player.Index].X - undoPoint2[args.Player.Index].X);
                int height = Math.Abs(undoPoint1[args.Player.Index].Y - undoPoint2[args.Player.Index].Y);
                int X, Y;
                for (int y = 0; y <= height; y++)
                {

                    for (int x = 0; x <= width; x++)
                    {

                        X = undoPoint1[args.Player.Index].X + x;
                        Y = undoPoint1[args.Player.Index].Y + y;
                        try
                        {
                            Main.tile[X, Y].active = (undoTiles[args.Player.Index])[X, Y].active;
                            try
                            {
                                if (findTileByID((undoTiles[args.Player.Index])[X, Y].type) != 255)
                                    Main.tile[X, Y].type = (undoTiles[args.Player.Index])[X, Y].type;
                            }
                            catch (NullReferenceException) { Main.tile[X, Y].active = false; }
                            try { Main.tile[X, Y].wall = (undoTiles[args.Player.Index])[X, Y].wall; }
                            catch (NullReferenceException) { Main.tile[X, Y].wall = 0; }
                            try { Main.tile[X, Y].frameNumber = (undoTiles[args.Player.Index])[X, Y].frameNumber; }
                            catch (NullReferenceException) { Main.tile[X, Y].frameNumber = 1; }
                            try { Main.tile[X, Y].frameX = (undoTiles[args.Player.Index])[X, Y].frameX; }
                            catch (NullReferenceException) { Main.tile[X, Y].frameX = 1; }
                            try { Main.tile[X, Y].frameY = (undoTiles[args.Player.Index])[X, Y].frameY; }
                            catch (NullReferenceException) { Main.tile[X, Y].frameY = 1; }
                            Main.tile[X, Y].checkingLiquid = false;
                            Main.tile[X, Y].lava = (undoTiles[args.Player.Index])[X, Y].lava;
                            try { Main.tile[X, Y].liquid = (undoTiles[args.Player.Index])[X, Y].liquid; }
                            catch (NullReferenceException) { Main.tile[X, Y].liquid = 0; }
                            Main.tile[X, Y].skipLiquid = (undoTiles[args.Player.Index])[X, Y].skipLiquid;
                            if ((Main.tile[X, Y].type == 53) || (Main.tile[X, Y].type == 253) || (Main.tile[X, Y].type == 254))
                                WorldGen.SquareTileFrame(X, Y, false);
                        }
                        catch (Exception) { }

                    }

                }
                for (int y = 0; y <= height; y++)
                {

                    for (int x = 0; x <= width; x++)
                    {

                        X = undoPoint1[args.Player.Index].X + x;
                        Y = undoPoint1[args.Player.Index].Y + y;
                        updateTile(X, Y);

                    }

                }
                args.Player.SendMessage("Success!");

            }

        }

        public static void LastArea(CommandArgs args)
        {

            if (lastArea1[args.Player.Index] != PointF.Empty)
            {

                if (lastArea2[args.Player.Index] != PointF.Empty)
                {

                    args.Player.TempPoints[0] = lastArea1[args.Player.Index];
                    args.Player.TempPoints[1] = lastArea2[args.Player.Index];
                    args.Player.SendMessage("Both temp points reset.");

                }
                else
                {

                    args.Player.TempPoints[0] = lastArea1[args.Player.Index];
                    args.Player.SendMessage("Temp point 1 reset.", System.Drawing.Color.Orange);

                }

            }
            else
            {

                args.Player.SendMessage("You haven't set any points in the past.", System.Drawing.Color.Red);

            }
            lastArea1[args.Player.Index] = System.Drawing.Point.Empty;
            lastArea2[args.Player.Index] = System.Drawing.Point.Empty;

        }
        public static void BrushStroke(CommandArgs args)
        {

            if (args.Parameters.Count > 0)
            {

                try
                {

                    brushStroke[args.Player.Index] = Convert.ToDouble(args.Parameters[0]);
                    args.Player.SendMessage("Your brushstroke size has been changed to " + brushStroke[args.Player.Index].ToString());

                }
                catch (Exception) { args.Player.SendMessage("You must give us a proper number.", System.Drawing.Color.Red); return; }

            }
            else
            {

                args.Player.SendMessage("Improper Syntax. Proper Syntax: /brushstroke size", System.Drawing.Color.Red);

            }

        }

        public static void Line(CommandArgs args)
        {

            if (args.Parameters.Count > 0)
            {
                if (!args.Player.TempPoints.Any(p => p == PointF.Empty))
                {

                    string theString = "";
                    for (int i = 0; i < args.Parameters.Count; i++)
                    {

                        if (i + 1 != args.Parameters.Count)
                        {
                            theString += args.Parameters[i] + " ";
                        }
                        else
                        {
                            theString += args.Parameters[i];
                        }

                    }
                    bool isWall = false;
                    byte tileType = findTileType(theString);
                    byte wallType = findWallType(theString);
                    isWall = ((tileType == 255) && (wallType != 255));
                    if ((tileType != 255) || (wallType != 255))
                    {
                        int x = Math.Min(args.Player.TempPoints[0].X, args.Player.TempPoints[1].X);
                        int y = Math.Min(args.Player.TempPoints[0].Y, args.Player.TempPoints[1].Y);
                        int width = Math.Abs(args.Player.TempPoints[0].X - args.Player.TempPoints[1].X);
                        int height = Math.Abs(args.Player.TempPoints[0].Y - args.Player.TempPoints[1].Y);
                        int trueX = args.Player.TempPoints[0].X;
                        int trueY = args.Player.TempPoints[0].Y;
                        int trueX2 = args.Player.TempPoints[1].X;
                        int trueY2 = args.Player.TempPoints[1].Y;
                        if (x == trueX)
                        {

                            if (y == trueY)
                            {

                                for (int y2 = 0; y2 <= height; y2++)
                                {

                                    for (int x2 = 0; x2 <= width; x2++)
                                    {

                                        if (Math.Abs(-(height / (double)width) * (x2) + y2) / Math.Sqrt(Math.Pow(height / (double)width, 2) + 1) <= brushStroke[args.Player.Index] / 2)
                                        {

                                            if (!isWall)
                                            {
                                                changeTile(x + x2, y + y2, tileType, 255);
                                            }
                                            else
                                            {
                                                changeTile(x + x2, y + y2, 255, wallType);
                                            }

                                        }

                                    }

                                }

                            }
                            else
                            {

                                for (int y2 = 0; y2 <= height; y2++)
                                {

                                    for (int x2 = 0; x2 <= width; x2++)
                                    {

                                        if (Math.Abs(-(-height / (double)width) * (x2) + y2 - height) / Math.Sqrt(Math.Pow(-height / (double)width, 2) + 1) <= brushStroke[args.Player.Index] / 2)
                                        {

                                            if (!isWall)
                                            {
                                                changeTile(x + x2, y + y2, tileType, 255);
                                            }
                                            else
                                            {
                                                changeTile(x + x2, y + y2, 255, wallType);
                                            }

                                        }

                                    }

                                }

                            }

                        }
                        else
                        {

                            if (y == trueY)
                            {

                                for (int y2 = 0; y2 <= height; y2++)
                                {

                                    for (int x2 = 0; x2 <= width; x2++)
                                    {

                                        if (Math.Abs(-(height / -(double)width) * (x2 - width) + y2) / Math.Sqrt(Math.Pow(height / -(double)width, 2) + 1) <= brushStroke[args.Player.Index] / 2)
                                        {

                                            if (!isWall)
                                            {
                                                changeTile(x + x2, y + y2, tileType, 255);
                                            }
                                            else
                                            {
                                                changeTile(x + x2, y + y2, 255, wallType);
                                            }

                                        }

                                    }

                                }

                            }
                            else
                            {

                                for (int y2 = 0; y2 <= height; y2++)
                                {

                                    for (int x2 = 0; x2 <= width; x2++)
                                    {

                                        if (Math.Abs(-(-height / -(double)width) * (x2 - width) + y2 - height) / Math.Sqrt(Math.Pow(-height / -(double)width, 2) + 1) <= brushStroke[args.Player.Index] / 2)
                                        {

                                            if (!isWall)
                                            {
                                                changeTile(x + x2, y + y2, tileType, 255);
                                            }
                                            else
                                            {
                                                changeTile(x + x2, y + y2, 255, wallType);
                                            }

                                        }

                                    }

                                }

                            }

                        }
                        for (int y2 = 0; y2 <= height; y2++)
                        {

                            for (int x2 = 0; x2 <= width; x2++)
                            {

                                updateTile(x + x2, y + y2);

                            }

                        }
                        lastArea1[args.Player.Index] = args.Player.TempPoints[0];
                        lastArea2[args.Player.Index] = args.Player.TempPoints[1];
                        args.Player.TempPoints[0] = System.Drawing.Point.Empty;
                        args.Player.TempPoints[1] = System.Drawing.Point.Empty;
                        args.Player.SendMessage("Tiles changed!");
                    }
                    else
                    {

                        args.Player.SendMessage("That is not a recognized tile type.", System.Drawing.Color.Red);

                    }
                }
                else
                {
                    args.Player.SendMessage("Points not set up yet", System.Drawing.Color.Red);
                }
            }
            else
            {

                args.Player.SendMessage("Improper syntax! Proper syntax: /line blocktype", System.Drawing.Color.Red);

            }

        }

        public static void Replace(CommandArgs args)
        {

            if (args.Parameters.Count > 1)
            {

                if (!args.Player.TempPoints.Any(p => p == PointF.Empty))
                {

                    string theString = "";
                    for (int i = 1; i < args.Parameters.Count; i++)
                    {

                        if (i + 1 != args.Parameters.Count)
                        {
                            theString += args.Parameters[i] + " ";
                        }
                        else
                        {
                            theString += args.Parameters[i];
                        }

                    }
                    bool isWall = false;
                    bool isWall2 = false;
                    byte tileType = findTileType(args.Parameters[0]);
                    byte tileType2 = findTileType(theString);
                    byte wallType = findWallType(args.Parameters[0]);
                    byte wallType2 = findWallType(theString);
                    isWall = ((tileType == 255) && (wallType != 255));
                    isWall2 = ((tileType2 == 255) && (wallType2 != 255));
                    if ((tileType != 255) || (wallType != 255))
                    {
                        if ((tileType2 != 255) || (wallType2 != 255))
                        {
                            int x = Math.Min(args.Player.TempPoints[0].X, args.Player.TempPoints[1].X);
                            int y = Math.Min(args.Player.TempPoints[0].Y, args.Player.TempPoints[1].Y);
                            int width = Math.Abs(args.Player.TempPoints[0].X - args.Player.TempPoints[1].X);
                            int height = Math.Abs(args.Player.TempPoints[0].Y - args.Player.TempPoints[1].Y);
                            for (int y2 = 0; y2 <= height; y2++)
                            {

                                for (int x2 = 0; x2 <= width; x2++)
                                {

                                    if (!isWall)
                                    {
                                        if (((Main.tile[x + x2, y + y2].type == tileType) && (Main.tile[x + x2, y + y2].active)) || ((tileType == 253) && (Main.tile[x + x2, y + y2].lava == false) && (Main.tile[x + x2, y + y2].liquid > 0)) || ((tileType == 254) && (Main.tile[x + x2, y + y2].lava == true) && (Main.tile[x + x2, y + y2].liquid > 0)))
                                        {
                                            if (!isWall2)
                                            {
                                                changeTile(x + x2, y + y2, tileType2, 255);
                                            }
                                            else
                                            {
                                                changeTile(x + x2, y + y2, 255, wallType2);
                                                Main.tile[x + x2, y + y2].active = false;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (Main.tile[x + x2, y + y2].wall == wallType)
                                        {
                                            if (!isWall2)
                                            {
                                                changeTile(x + x2, y + y2, tileType2, 255);
                                                Main.tile[x + x2, y + y2].wall = 0;
                                            }
                                            else
                                            {
                                                changeTile(x + x2, y + y2, 255, wallType2);
                                            }
                                        }
                                    }

                                }

                            }
                            for (int y2 = 0; y2 <= height; y2++)
                            {

                                for (int x2 = 0; x2 <= width; x2++)
                                {

                                    updateTile(x + x2, y + y2);

                                }

                            }
                            lastArea1[args.Player.Index] = args.Player.TempPoints[0];
                            lastArea2[args.Player.Index] = args.Player.TempPoints[1];
                            args.Player.TempPoints[0] = System.Drawing.Point.Empty;
                            args.Player.TempPoints[1] = System.Drawing.Point.Empty;
                            args.Player.SendMessage("Tiles changed!");
                        }
                        else
                        {


                            args.Player.SendMessage(theString + " is not a recognized tile type.", System.Drawing.Color.Red);

                        }
                    }
                    else
                    {

                        args.Player.SendMessage(args.Parameters[0] + " is not a recognized tile type.", System.Drawing.Color.Red);

                    }
                }
                else
                {
                    args.Player.SendMessage("Points not set up yet", System.Drawing.Color.Red);
                }

            }
            else
            {

                args.Player.SendMessage("Improper Syntax. Proper Syntax: /replace tiletype1 tiletype2", System.Drawing.Color.Red);

            }

        }

        public static void Oval(CommandArgs args)
        {

            if (args.Parameters.Count > 0)
            {
                if (!args.Player.TempPoints.Any(p => p == PointF.Empty))
                {
                    
                    string theString = "";
                    for (int i = 0; i < args.Parameters.Count; i++)
                    {

                        if (i + 1 != args.Parameters.Count)
                        {
                            theString += args.Parameters[i] + " ";
                        }
                        else
                        {
                            theString += args.Parameters[i];
                        }

                    }
                    bool isWall = false;
                    byte tileType = findTileType(theString);
                    byte wallType = findWallType(theString);
                    isWall = ((tileType == 255) && (wallType != 255));
                    if ((tileType != 255) || (wallType != 255))
                    {
                        int x = Math.Min(args.Player.TempPoints[0].X, args.Player.TempPoints[1].X);
                        int y = Math.Min(args.Player.TempPoints[0].Y, args.Player.TempPoints[1].Y);
                        int width = Math.Abs(args.Player.TempPoints[0].X - args.Player.TempPoints[1].X);
                        int height = Math.Abs(args.Player.TempPoints[0].Y - args.Player.TempPoints[1].Y);
                        for (int y2 = 0; y2 <= height; y2++)
                        {

                            for (int x2 = 0; x2 <= width; x2++)
                            {
                                
                                if (Math.Pow((x2 - width / 2.0) / (width / 2.0), 2) + Math.Pow((y2 - height / 2.0) / (height / 2.0), 2) <= 1.0)
                                {

                                    if (!isWall)
                                    {
                                        changeTile(x + x2, y + y2, tileType, 255);
                                    }
                                    else
                                    {
                                        changeTile(x + x2, y + y2, 255, wallType);
                                    }

                                }

                            }

                        }
                        for (int y2 = 0; y2 <= height; y2++)
                        {

                            for (int x2 = 0; x2 <= width; x2++)
                            {

                                updateTile(x + x2, y + y2);

                            }

                        }
                        lastArea1[args.Player.Index] = args.Player.TempPoints[0];
                        lastArea2[args.Player.Index] = args.Player.TempPoints[1];
                        args.Player.TempPoints[0] = System.Drawing.Point.Empty;
                        args.Player.TempPoints[1] = System.Drawing.Point.Empty;
                        args.Player.SendMessage("Tiles changed!");
                    }
                    else
                    {

                        args.Player.SendMessage("That is not a recognized tile type.", System.Drawing.Color.Red);

                    }
                }
                else
                {
                    args.Player.SendMessage("Points not set up yet", System.Drawing.Color.Red);
                }
            }
            else
            {

                args.Player.SendMessage("Improper syntax! Proper syntax: /oval blocktype", System.Drawing.Color.Red);

            }

        }

        public static void SemiOval(CommandArgs args)
        {

            if (args.Parameters.Count > 1)
            {
                if (!args.Player.TempPoints.Any(p => p == PointF.Empty))
                {

                    string theString = "";
                    for (int i = 1; i < args.Parameters.Count; i++)
                    {

                        if (i + 1 != args.Parameters.Count)
                        {
                            theString += args.Parameters[i] + " ";
                        }
                        else
                        {
                            theString += args.Parameters[i];
                        }

                    }
                    bool isWall = false;
                    byte tileType = findTileType(theString);
                    byte wallType = findWallType(theString);
                    isWall = ((tileType == 255) && (wallType != 255));
                    if ((tileType != 255) || (wallType != 255))
                    {
                        int x = Math.Min(args.Player.TempPoints[0].X, args.Player.TempPoints[1].X);
                        int y = Math.Min(args.Player.TempPoints[0].Y, args.Player.TempPoints[1].Y);
                        int width = Math.Abs(args.Player.TempPoints[0].X - args.Player.TempPoints[1].X);
                        int height = Math.Abs(args.Player.TempPoints[0].Y - args.Player.TempPoints[1].Y);
                        for (int y2 = 0; y2 <= height; y2++)
                        {

                            for (int x2 = 0; x2 <= width; x2++)
                            {

                                switch (args.Parameters[0].ToLower())
                                {

                                    case "left": if (Math.Pow((x2 - width) / Convert.ToDouble(width), 2) + Math.Pow((y2 - height / 2.0) / (height / 2.0), 2) <= 1.0)
                                        {

                                            if (!isWall)
                                            {
                                                changeTile(x + x2, y + y2, tileType, 255);
                                            }
                                            else
                                            {
                                                changeTile(x + x2, y + y2, 255, wallType);
                                            }

                                        }
                                        break;
                                    case "right": if (Math.Pow((x2) / Convert.ToDouble(width), 2) + Math.Pow((y2 - height / 2.0) / (height / 2.0), 2) <= 1.0)
                                        {

                                            if (!isWall)
                                            {
                                                changeTile(x + x2, y + y2, tileType, 255);
                                            }
                                            else
                                            {
                                                changeTile(x + x2, y + y2, 255, wallType);
                                            }

                                        }
                                        break;
                                    case "top": if (Math.Pow((x2 - width / 2.0) / (width / 2.0), 2) + Math.Pow((y2 - height) / Convert.ToDouble(height), 2) <= 1.0)
                                        {

                                            if (!isWall)
                                            {
                                                changeTile(x + x2, y + y2, tileType, 255);
                                            }
                                            else
                                            {
                                                changeTile(x + x2, y + y2, 255, wallType);
                                            }

                                        }
                                        break;
                                    case "bottom": if (Math.Pow((x2 - width / 2.0) / (width / 2.0), 2) + Math.Pow((y2) / Convert.ToDouble(height), 2) <= 1.0)
                                        {

                                            if (!isWall)
                                            {
                                                changeTile(x + x2, y + y2, tileType, 255);
                                            }
                                            else
                                            {
                                                changeTile(x + x2, y + y2, 255, wallType);
                                            }

                                        }
                                        break;
                                    default: args.Player.SendMessage("Improper syntax! Proper syntax: /semioval top|left|bottom|right blocktype", System.Drawing.Color.Red); return;

                                }

                            }

                        }
                        for (int y2 = 0; y2 <= height; y2++)
                        {

                            for (int x2 = 0; x2 <= width; x2++)
                            {

                                updateTile(x + x2, y + y2);

                            }

                        }
                        lastArea1[args.Player.Index] = args.Player.TempPoints[0];
                        lastArea2[args.Player.Index] = args.Player.TempPoints[1];
                        args.Player.TempPoints[0] = System.Drawing.Point.Empty;
                        args.Player.TempPoints[1] = System.Drawing.Point.Empty;
                        args.Player.SendMessage("Tiles changed!");
                    }
                    else
                    {

                        args.Player.SendMessage("That is not a recognized tile type.", System.Drawing.Color.Red);

                    }
                }
                else
                {
                    args.Player.SendMessage("Points not set up yet", System.Drawing.Color.Red);
                }
            }
            else
            {

                args.Player.SendMessage("Improper syntax! Proper syntax: /semioval top|left|bottom|right blocktype", System.Drawing.Color.Red);

            }

        }

        public static void SemiOvalOutline(CommandArgs args)
        {

            if (args.Parameters.Count > 1)
            {
                if (!args.Player.TempPoints.Any(p => p == PointF.Empty))
                {

                    string theString = "";
                    for (int i = 1; i < args.Parameters.Count; i++)
                    {

                        if (i + 1 != args.Parameters.Count)
                        {
                            theString += args.Parameters[i] + " ";
                        }
                        else
                        {
                            theString += args.Parameters[i];
                        }

                    }
                    bool isWall = false;
                    byte tileType = findTileType(theString);
                    byte wallType = findWallType(theString);
                    isWall = ((tileType == 255) && (wallType != 255));
                    if ((tileType != 255) || (wallType != 255))
                    {
                        int x = Math.Min(args.Player.TempPoints[0].X, args.Player.TempPoints[1].X);
                        int y = Math.Min(args.Player.TempPoints[0].Y, args.Player.TempPoints[1].Y);
                        int width = Math.Abs(args.Player.TempPoints[0].X - args.Player.TempPoints[1].X);
                        int height = Math.Abs(args.Player.TempPoints[0].Y - args.Player.TempPoints[1].Y);
                        for (int y2 = 0; y2 <= height; y2++)
                        {

                            for (int x2 = 0; x2 <= width; x2++)
                            {

                                switch (args.Parameters[0].ToLower())
                                {

                                    case "left": if ((Math.Pow((x2 - width / 1.0) / (width / 1.0), 2) + Math.Pow((y2 - height / 2.0) / (height / 2.0), 2) <= 1.0) && (Math.Pow((x2 - width / 1.0) / (width / 1.0), 2) + Math.Pow((y2 - height / 2.0) / (height / 2.0), 2) >= Convert.ToDouble(width * 2 >= height) * ((width * 2.0 - 7.5) / (width * 2.0)) + Convert.ToDouble(width * 2 < height) * ((height - 7.5) / height)))
                                        {

                                            if (!isWall)
                                            {
                                                changeTile(x + x2, y + y2, tileType, 255);
                                            }
                                            else
                                            {
                                                changeTile(x + x2, y + y2, 255, wallType);
                                            }

                                        }
                                        break;
                                    case "right": if ((Math.Pow((x2) / (width / 1.0), 2) + Math.Pow((y2 - height / 2.0) / (height / 2.0), 2) <= 1.0) && (Math.Pow((x2) / (width / 1.0), 2) + Math.Pow((y2 - height / 2.0) / (height / 2.0), 2) >= Convert.ToDouble(width * 2 >= height) * ((width * 2.0 - 7.5) / (width * 2.0)) + Convert.ToDouble(width * 2 < height) * ((height - 7.5) / height)))
                                        {

                                            if (!isWall)
                                            {
                                                changeTile(x + x2, y + y2, tileType, 255);
                                            }
                                            else
                                            {
                                                changeTile(x + x2, y + y2, 255, wallType);
                                            }

                                        }
                                        break;
                                    case "bottom": if ((Math.Pow((x2 - width / 2.0) / (width / 2.0), 2) + Math.Pow((y2) / (height / 1.0), 2) <= 1.0) && (Math.Pow((x2 - width / 2.0) / (width / 2.0), 2) + Math.Pow((y2) / (height / 1.0), 2) >= Convert.ToDouble(width >= height * 2) * ((width - 7.5) / width) + Convert.ToDouble(width < height * 2) * ((height * 2.0 - 7.5) / (height * 2.0))))
                                        {

                                            if (!isWall)
                                            {
                                                changeTile(x + x2, y + y2, tileType, 255);
                                            }
                                            else
                                            {
                                                changeTile(x + x2, y + y2, 255, wallType);
                                            }

                                        }
                                        break;
                                    case "top": if ((Math.Pow((x2 - width / 2.0) / (width / 2.0), 2) + Math.Pow((y2 - height / 1.0) / (height / 1.0), 2) <= 1.0) && (Math.Pow((x2 - width / 2.0) / (width / 2.0), 2) + Math.Pow((y2 - height / 1.0) / (height / 1.0), 2) >= Convert.ToDouble(width >= height * 2) * ((width - 7.5) / width) + Convert.ToDouble(width < height * 2) * ((height * 2.0 - 7.5) / (height * 2.0))))
                                        {

                                            if (!isWall)
                                            {
                                                changeTile(x + x2, y + y2, tileType, 255);
                                            }
                                            else
                                            {
                                                changeTile(x + x2, y + y2, 255, wallType);
                                            }

                                        }
                                        break;
                                    default: args.Player.SendMessage("Improper syntax! Proper syntax: /semiovaloutline top|left|bottom|right blocktype", System.Drawing.Color.Red); return;

                                }

                            }

                        }
                        for (int y2 = 0; y2 <= height; y2++)
                        {

                            for (int x2 = 0; x2 <= width; x2++)
                            {

                                updateTile(x + x2, y + y2);

                            }

                        }
                        lastArea1[args.Player.Index] = args.Player.TempPoints[0];
                        lastArea2[args.Player.Index] = args.Player.TempPoints[1];
                        args.Player.TempPoints[0] = System.Drawing.Point.Empty;
                        args.Player.TempPoints[1] = System.Drawing.Point.Empty;
                        args.Player.SendMessage("Tiles changed!");
                    }
                    else
                    {

                        args.Player.SendMessage("That is not a recognized tile type.", System.Drawing.Color.Red);

                    }
                }
                else
                {
                    args.Player.SendMessage("Points not set up yet", System.Drawing.Color.Red);
                }
            }
            else
            {

                args.Player.SendMessage("Improper syntax! Proper syntax: /semiovaloutline top|left|bottom|right blocktype", System.Drawing.Color.Red);

            }

        }

        public static void OvalOutline(CommandArgs args)
        {

            if (args.Parameters.Count > 0)
            {
                if (!args.Player.TempPoints.Any(p => p == PointF.Empty))
                {

                    string theString = "";
                    for (int i = 0; i < args.Parameters.Count; i++)
                    {

                        if (i + 1 != args.Parameters.Count)
                        {
                            theString += args.Parameters[i] + " ";
                        }
                        else
                        {
                            theString += args.Parameters[i];
                        }

                    }
                    bool isWall = false;
                    byte tileType = findTileType(theString);
                    byte wallType = findWallType(theString);
                    isWall = ((tileType == 255) && (wallType != 255));
                    if ((tileType != 255) || (wallType != 255))
                    {
                        int x = Math.Min(args.Player.TempPoints[0].X, args.Player.TempPoints[1].X);
                        int y = Math.Min(args.Player.TempPoints[0].Y, args.Player.TempPoints[1].Y);
                        int width = Math.Abs(args.Player.TempPoints[0].X - args.Player.TempPoints[1].X);
                        int height = Math.Abs(args.Player.TempPoints[0].Y - args.Player.TempPoints[1].Y);
                        for (int y2 = 0; y2 <= height; y2++)
                        {

                            for (int x2 = 0; x2 <= width; x2++)
                            {

                                if ((Math.Pow((x2 - width / 2.0) / (width / 2.0), 2) + Math.Pow((y2 - height / 2.0) / (height / 2.0), 2) <= 1.0)&&(Math.Pow((x2 - width / 2.0) / (width / 2.0), 2) + Math.Pow((y2 - height / 2.0) / (height / 2.0), 2) >= Convert.ToDouble(width>=height)*((width-7.5)/width) + Convert.ToDouble(width<height)*((height-7.5)/height))) {

                                    if (!isWall)
                                    {
                                        changeTile(x + x2, y + y2, tileType, 255);
                                    }
                                    else
                                    {
                                        changeTile(x + x2, y + y2, 255, wallType);
                                    }

                                }

                            }

                        }
                        for (int y2 = 0; y2 <= height; y2++)
                        {

                            for (int x2 = 0; x2 <= width; x2++)
                            {

                                updateTile(x + x2, y + y2);

                            }

                        }
                        lastArea1[args.Player.Index] = args.Player.TempPoints[0];
                        lastArea2[args.Player.Index] = args.Player.TempPoints[1];
                        args.Player.TempPoints[0] = System.Drawing.Point.Empty;
                        args.Player.TempPoints[1] = System.Drawing.Point.Empty;
                        args.Player.SendMessage("Tiles changed!");
                    }
                    else
                    {

                        args.Player.SendMessage("That is not a recognized tile type.", System.Drawing.Color.Red);

                    }
                }
                else
                {
                    args.Player.SendMessage("Points not set up yet", System.Drawing.Color.Red);
                }
            }
            else
            {

                args.Player.SendMessage("Improper syntax! Proper syntax: /ovaloutline blocktype", System.Drawing.Color.Red);

            }

        }

        public static void Paste(CommandArgs args)
        {

            if ((copyW[args.Player.Index] != 0) || (copyH[args.Player.Index] != 0))
            {

                if (args.Player.TempPoints[0] != PointF.Empty)
                {

                    int X, Y;
                    int x = args.Player.TempPoints[0].X;
                    int y = args.Player.TempPoints[0].Y;
                    int width = copyW[args.Player.Index];
                    int height = copyH[args.Player.Index];
                    int i = args.Player.Index;
                    if (rectCollision(copyX[i], copyY[i], copyX[i] + width, copyY[i] + height, x, y, x + width, y + height))
                    {

                        args.Player.SendMessage("You cannot paste into the cut/copied area.", System.Drawing.Color.Red);
                        return;

                    }
                    for (int y2 = 0; y2 <= height; y2++)
                    {

                        for (int x2 = 0; x2 <= width; x2++)
                        {

                            X = copyX[i] + x2;
                            Y = copyY[i] + y2;
                            try
                            {
                                if (clipboardType[args.Player.Index] != 2)
                                Main.tile[x + x2, y + y2].active = Main.tile[X,Y].active;
                                try
                                {
                                    if (findTileByID(Main.tile[X, Y].type) != 255)
                                    if (clipboardType[args.Player.Index] != 2)
                                    Main.tile[x + x2, y + y2].type = Main.tile[X, Y].type;
                                }
                                catch (NullReferenceException) { Main.tile[x + x2, y + y2].active = false; }
                                try { if (clipboardType[args.Player.Index] != 1) Main.tile[x + x2, y + y2].wall = Main.tile[X, Y].wall; }
                                catch (NullReferenceException) { Main.tile[x + x2, y + y2].wall = 0; }
                                try { Main.tile[x + x2, y + y2].frameNumber = Main.tile[X, Y].frameNumber; }
                                catch (NullReferenceException) { Main.tile[x + x2, y + y2].frameNumber = 1; }
                                try { Main.tile[x + x2, y + y2].frameX = Main.tile[X, Y].frameX; }
                                catch (NullReferenceException) { Main.tile[x + x2, y + y2].frameX = 1; }
                                try { Main.tile[x + x2, y + y2].frameY = Main.tile[X, Y].frameY; }
                                catch (NullReferenceException) { Main.tile[x + x2, y + y2].frameY = 1; }
                                Main.tile[x + x2, y + y2].checkingLiquid = false;
                                Main.tile[x + x2, y + y2].lava = Main.tile[X, Y].lava;
                                try { Main.tile[x + x2, y + y2].liquid = Main.tile[X, Y].liquid; }
                                catch (NullReferenceException) { Main.tile[x + x2, y + y2].liquid = 0; }
                                Main.tile[x + x2, y + y2].skipLiquid = Main.tile[X, Y].skipLiquid;
                                if ((Main.tile[x + x2, y + y2].type == 53) || (Main.tile[x + x2, y + y2].type >= 253))
                                WorldGen.SquareTileFrame(x + x2, y + y2, false);
                            }
                            catch (Exception) { }

                        }

                    }
                    for (int y2 = 0; y2 <= height; y2++)
                    {

                        for (int x2 = 0; x2 <= width; x2++)
                        {

                            X = copyX[i] + x2;
                            Y = copyY[i] + y2;
                            try
                            {
                                if (findTileByID(Main.tile[X, Y].type) == 255)
                                    Main.tile[x + x2, y + y2].type = Main.tile[X, Y].type;
                            }
                            catch (Exception) { Main.tile[x + x2, y + y2].active = false; }

                        }

                    }
                    for (int y2 = 0; y2 <= height; y2++)
                    {

                        for (int x2 = 0; x2 <= width; x2++)
                        {

                            X = copyX[i] + x2;
                            Y = copyY[i] + y2;
                            try
                            {
                                if (findTileByID(Main.tile[X, Y].type) != 255)
                                    try
                                    {
                                        updateTile(x + x2, y + y2);
                                    }
                                    catch (Exception) { }
                            }
                            catch (Exception)
                            {
                                try
                                {
                                    updateTile(x + x2, y + y2);
                                }
                                catch (Exception) { }
                            }

                        }

                    }
                    for (int y2 = 0; y2 <= height; y2++)
                    {

                        for (int x2 = 0; x2 <= width; x2++)
                        {

                            X = copyX[i] + x2;
                            Y = copyY[i] + y2;
                            try
                            {
                                if (findTileByID(Main.tile[X, Y].type) == 255)
                                {
                                    updateTile(x + x2, y + y2);
                                }
                            }
                            catch (Exception) { }

                        }
                    
                    }
                    if (cut[args.Player.Index])
                    {

                        x = copyX[args.Player.Index];
                        y = copyY[args.Player.Index];
                        for (int y2 = 0; y2 <= height; y2++)
                        {

                            for (int x2 = 0; x2 <= width; x2++)
                            {

                                if (clipboardType[args.Player.Index] != 2)
                                Main.tile[x + x2, y + y2].active = false;
                                if (clipboardType[args.Player.Index] != 1)
                                Main.tile[x + x2, y + y2].wall = 0;
                                Main.tile[x + x2, y + y2].lava = false;
                                Main.tile[x + x2, y + y2].liquid = 0;
                                Main.tile[x + x2, y + y2].skipLiquid = true;
                                Main.tile[x + x2, y + y2].checkingLiquid = false;
                                if ((Main.tile[x + x2, y + y2].type == 53) || (Main.tile[x + x2, y + y2].type >= 253))
                                WorldGen.SquareTileFrame(x + x2, y + y2, false);

                            }

                        }
                        for (int y2 = 0; y2 <= height; y2++)
                        {

                            for (int x2 = 0; x2 <= width; x2++)
                            {

                                updateTile(x + x2, y + y2);

                            }

                        }
                        args.Player.SendMessage("Tiles cut.");
                        copyX[args.Player.Index] = 0;
                        copyY[args.Player.Index] = 0;
                        copyW[args.Player.Index] = 0;
                        copyH[args.Player.Index] = 0;

                    }
                    lastArea1[args.Player.Index] = args.Player.TempPoints[0];
                    lastArea2[args.Player.Index] = args.Player.TempPoints[1];
                    args.Player.TempPoints[0] = System.Drawing.Point.Empty;
                    args.Player.TempPoints[1] = System.Drawing.Point.Empty;
                    args.Player.SendMessage("You have pasted from the clipboard.");
                }
                else
                {
                    args.Player.SendMessage("Point not set up yet", System.Drawing.Color.Red);
                }
            }
            else
            {

                args.Player.SendMessage("You need to copy something first!", System.Drawing.Color.Red);

            }

        }

        public static void Copy(CommandArgs args)
        {

            if (args.Parameters.Count > 0)
            {

                switch (args.Parameters[0].ToLower())
                {

                    case "all": clipboardType[args.Player.Index] = 0; break;
                    case "front": clipboardType[args.Player.Index] = 1; break;
                    case "back": clipboardType[args.Player.Index] = 2; break;
                    default: args.Player.SendMessage("Improper syntax.  Proper Syntax: /copy [all|front|back]", System.Drawing.Color.Red); return;

                }

            }
            else
            {

                clipboardType[args.Player.Index] = 0;

            }
            cut[args.Player.Index] = false;
            if (!args.Player.TempPoints.Any(p => p == PointF.Empty))
            {

                copyX[args.Player.Index] = Math.Min(args.Player.TempPoints[0].X, args.Player.TempPoints[1].X);
                copyY[args.Player.Index] = Math.Min(args.Player.TempPoints[0].Y, args.Player.TempPoints[1].Y);
                copyW[args.Player.Index] = Math.Abs(args.Player.TempPoints[0].X - args.Player.TempPoints[1].X);
                copyH[args.Player.Index] = Math.Abs(args.Player.TempPoints[0].Y - args.Player.TempPoints[1].Y);
                lastArea1[args.Player.Index] = args.Player.TempPoints[0];
                lastArea2[args.Player.Index] = args.Player.TempPoints[1];
                args.Player.TempPoints[0] = System.Drawing.Point.Empty;
                args.Player.TempPoints[1] = System.Drawing.Point.Empty;
                args.Player.SendMessage("You have successfully copied to the clipboard.");
            }
            else
            {
                args.Player.SendMessage("Points not set up yet", System.Drawing.Color.Red);
            }

        }

        public static void Cut(CommandArgs args)
        {

            if (args.Parameters.Count > 0)
            {

                switch (args.Parameters[0].ToLower())
                {

                    case "all": clipboardType[args.Player.Index] = 0; break;
                    case "front": clipboardType[args.Player.Index] = 1; break;
                    case "back": clipboardType[args.Player.Index] = 2; break;
                    default: args.Player.SendMessage("Improper syntax.  Proper Syntax: /cut [all|front|back]", System.Drawing.Color.Red); return;

                }

            }
            else
            {

                clipboardType[args.Player.Index] = 0;

            }
            cut[args.Player.Index] = true;
            if (!args.Player.TempPoints.Any(p => p == PointF.Empty))
            {

                copyX[args.Player.Index] = Math.Min(args.Player.TempPoints[0].X, args.Player.TempPoints[1].X);
                copyY[args.Player.Index] = Math.Min(args.Player.TempPoints[0].Y, args.Player.TempPoints[1].Y);
                copyW[args.Player.Index] = Math.Abs(args.Player.TempPoints[0].X - args.Player.TempPoints[1].X);
                copyH[args.Player.Index] = Math.Abs(args.Player.TempPoints[0].Y - args.Player.TempPoints[1].Y);
                lastArea1[args.Player.Index] = args.Player.TempPoints[0];
                lastArea2[args.Player.Index] = args.Player.TempPoints[1];
                args.Player.TempPoints[0] = System.Drawing.Point.Empty;
                args.Player.TempPoints[1] = System.Drawing.Point.Empty;
                args.Player.SendMessage("You have successfully cut to the clipboard.");
            }
            else
            {
                args.Player.SendMessage("Points not set up yet", System.Drawing.Color.Red);
            }

        }

        public static void ClearArea(CommandArgs args)
        {

            if (args.Parameters.Count >= 1)
            {
                if (!args.Player.TempPoints.Any(p => p == PointF.Empty))
                {

                    int x = Math.Min(args.Player.TempPoints[0].X, args.Player.TempPoints[1].X);
                    int y = Math.Min(args.Player.TempPoints[0].Y, args.Player.TempPoints[1].Y);
                    int width = Math.Abs(args.Player.TempPoints[0].X - args.Player.TempPoints[1].X);
                    int height = Math.Abs(args.Player.TempPoints[0].Y - args.Player.TempPoints[1].Y);
                    switch (args.Parameters[0].ToLower())
                    {
                        case "all": for (int y2 = 0; y2 <= height; y2++)
                            {

                                for (int x2 = 0; x2 <= width; x2++)
                                {

                                    Main.tile[x + x2, y + y2].active = false;
                                    Main.tile[x + x2, y + y2].wall = 0;
                                    Main.tile[x + x2, y + y2].skipLiquid = true;
                                    Main.tile[x + x2, y + y2].liquid = 0;

                                }

                            } break;
                        case "back": for (int y2 = 0; y2 <= height; y2++)
                            {

                                for (int x2 = 0; x2 <= width; x2++)
                                {

                                    Main.tile[x + x2, y + y2].wall = 0;

                                }

                            } break;
                        case "front": for (int y2 = 0; y2 <= height; y2++)
                            {

                                for (int x2 = 0; x2 <= width; x2++)
                                {

                                    Main.tile[x + x2, y + y2].active = false;
                                    Main.tile[x + x2, y + y2].skipLiquid = true;
                                    Main.tile[x + x2, y + y2].liquid = 0;

                                }

                            } break;
                        default: args.Player.SendMessage("Improper Syntax.  Proper Syntax: /cleararea [all|back|front]"); return;
                    }
                    for (int y2 = 0; y2 <= height; y2++)
                    {

                        for (int x2 = 0; x2 <= width; x2++)
                        {

                            updateTile(x + x2, y + y2);

                        }

                    }
                    lastArea1[args.Player.Index] = args.Player.TempPoints[0];
                    lastArea2[args.Player.Index] = args.Player.TempPoints[1];
                    args.Player.TempPoints[0] = System.Drawing.Point.Empty;
                    args.Player.TempPoints[1] = System.Drawing.Point.Empty;
                    args.Player.SendMessage("Tiles cleared!");
                }
                else
                {
                    args.Player.SendMessage("Points not set up yet", System.Drawing.Color.Red);
                }
            }
            else
            {

                if (!args.Player.TempPoints.Any(p => p == PointF.Empty))
                {

                    int x = Math.Min(args.Player.TempPoints[0].X, args.Player.TempPoints[1].X);
                    int y = Math.Min(args.Player.TempPoints[0].Y, args.Player.TempPoints[1].Y);
                    int width = Math.Abs(args.Player.TempPoints[0].X - args.Player.TempPoints[1].X);
                    int height = Math.Abs(args.Player.TempPoints[0].Y - args.Player.TempPoints[1].Y);
                    for (int y2 = 0; y2 <= height; y2++)
                    {

                        for (int x2 = 0; x2 <= width; x2++)
                        {

                            Main.tile[x + x2, y + y2].active = false;
                            Main.tile[x + x2, y + y2].wall = 0;
                            Main.tile[x + x2, y + y2].skipLiquid = true;
                            Main.tile[x + x2, y + y2].liquid = 0;

                        }

                    }
                    for (int y2 = 0; y2 <= height; y2++)
                    {

                        for (int x2 = 0; x2 <= width; x2++)
                        {

                            updateTile(x + x2, y + y2);

                        }

                    }
                    lastArea1[args.Player.Index] = args.Player.TempPoints[0];
                    lastArea2[args.Player.Index] = args.Player.TempPoints[1];
                    args.Player.TempPoints[0] = System.Drawing.Point.Empty;
                    args.Player.TempPoints[1] = System.Drawing.Point.Empty;
                    args.Player.SendMessage("Tiles cleared!");
                }
                else
                {
                    args.Player.SendMessage("Points not set up yet", System.Drawing.Color.Red);
                }

            }
        }

        public static void Rectangle(CommandArgs args)
        {
            
            if (args.Parameters.Count > 0)
            {
                if (!args.Player.TempPoints.Any(p => p == PointF.Empty))
                {
                    string theString = "";
                    for (int i = 0; i < args.Parameters.Count; i++)
                    {

                        if (i + 1 != args.Parameters.Count)
                        {
                            theString += args.Parameters[i] + " ";
                        }
                        else
                        {
                            theString += args.Parameters[i];
                        }

                    }
                    bool isWall = false;
                    byte tileType = findTileType(theString);
                    byte wallType = findWallType(theString);
                    isWall = ((tileType == 255) && (wallType != 255));
                    if ((tileType != 255) || (wallType != 255))
                    {
                        int x = Math.Min(args.Player.TempPoints[0].X, args.Player.TempPoints[1].X);
                        int y = Math.Min(args.Player.TempPoints[0].Y, args.Player.TempPoints[1].Y);
                        int width = Math.Abs(args.Player.TempPoints[0].X - args.Player.TempPoints[1].X);
                        int height = Math.Abs(args.Player.TempPoints[0].Y - args.Player.TempPoints[1].Y);
                        undoTiles[args.Player.Index] = Main.tile;
                        undoPoint1[args.Player.Index].X = x;
                        undoPoint1[args.Player.Index].Y = y;
                        undoPoint2[args.Player.Index].X = x + width;
                        undoPoint2[args.Player.Index].Y = y + height;
                        for (int y2 = 0; y2 <= height; y2++)
                        {

                            for (int x2 = 0; x2 <= width; x2++)
                            {

                                if (!isWall)
                                {
                                    changeTile(x + x2, y + y2, tileType, 255);
                                }
                                else
                                {
                                    changeTile(x + x2, y + y2, 255, wallType);
                                }

                            }

                        }
                        for (int y2 = 0; y2 <= height; y2++)
                        {

                            for (int x2 = 0; x2 <= width; x2++)
                            {

                                updateTile(x + x2, y + y2);

                            }

                        }
                        lastArea1[args.Player.Index] = args.Player.TempPoints[0];
                        lastArea2[args.Player.Index] = args.Player.TempPoints[1];
                        args.Player.TempPoints[0] = System.Drawing.Point.Empty;
                        args.Player.TempPoints[1] = System.Drawing.Point.Empty;
                        args.Player.SendMessage("Tiles changed!"); return;
                    }
                    else
                    {

                        args.Player.SendMessage("That is not a recognized tile type.", System.Drawing.Color.Red);

                    }
                }
                else
                {
                    args.Player.SendMessage("Points not set up yet", System.Drawing.Color.Red);
                }
            }
            else
            {

                args.Player.SendMessage("Improper syntax! Proper syntax: /rectangle blocktype", System.Drawing.Color.Red);

            }
        }

        public static void RectangleOutline(CommandArgs args)
        {

            if (args.Parameters.Count > 0)
            {
                if (!args.Player.TempPoints.Any(p => p == PointF.Empty))
                {
                    string theString = "";
                    for (int i = 0; i < args.Parameters.Count; i++)
                    {

                        if (i + 1 != args.Parameters.Count)
                        {
                            theString += args.Parameters[i] + " ";
                        }
                        else
                        {
                            theString += args.Parameters[i];
                        }

                    }
                    bool isWall = false;
                    byte tileType = findTileType(theString);
                    byte wallType = findWallType(theString);
                    isWall = ((tileType == 255) && (wallType != 255));
                    if ((tileType != 255) || (wallType != 255))
                    {
                        int x = Math.Min(args.Player.TempPoints[0].X, args.Player.TempPoints[1].X);
                        int y = Math.Min(args.Player.TempPoints[0].Y, args.Player.TempPoints[1].Y);
                        int width = Math.Abs(args.Player.TempPoints[0].X - args.Player.TempPoints[1].X);
                        int height = Math.Abs(args.Player.TempPoints[0].Y - args.Player.TempPoints[1].Y);
                        for (int y2 = -(int)(brushStroke[args.Player.Index] / 2); y2 <= height + (int)(brushStroke[args.Player.Index] / 2); y2++)
                        {

                            for (int x2 = -(int)(brushStroke[args.Player.Index] / 2); x2 <= width + (int)(brushStroke[args.Player.Index] / 2); x2++)
                            {

                                if ((Math.Abs(x2) <= brushStroke[args.Player.Index] / 2) || (Math.Abs(y2) <= brushStroke[args.Player.Index] / 2) || (Math.Abs(width - x2) <= brushStroke[args.Player.Index] / 2) || (Math.Abs(height - y2) <= brushStroke[args.Player.Index] / 2))
                                {

                                    if (!isWall)
                                    {
                                        changeTile(x + x2, y + y2, tileType, 255);
                                    }
                                    else
                                    {
                                        changeTile(x + x2, y + y2, 255, wallType);
                                    }

                                }

                            }

                        }
                        for (int y2 = -(int)(brushStroke[args.Player.Index] / 2); y2 <= height + (int)(brushStroke[args.Player.Index] / 2); y2++)
                        {

                            for (int x2 = -(int)(brushStroke[args.Player.Index] / 2); x2 <= width + (int)(brushStroke[args.Player.Index] / 2); x2++)
                            {

                                updateTile(x + x2, y + y2);

                            }

                        }
                        lastArea1[args.Player.Index] = args.Player.TempPoints[0];
                        lastArea2[args.Player.Index] = args.Player.TempPoints[1];
                        args.Player.TempPoints[0] = System.Drawing.Point.Empty;
                        args.Player.TempPoints[1] = System.Drawing.Point.Empty;
                        args.Player.SendMessage("Tiles changed!");
                    }
                    else
                    {

                        args.Player.SendMessage("That is not a recognized tile type.", System.Drawing.Color.Red);

                    }
                }
                else
                {
                    args.Player.SendMessage("Points not set up yet", System.Drawing.Color.Red);
                }
            }
            else
            {

                args.Player.SendMessage("Improper syntax! Proper syntax: /rectangleoutline blocktype", System.Drawing.Color.Red);

            }
        }

        public static void Circle(CommandArgs args)
        {

            if (args.Parameters.Count > 1)
            {
                if (args.Player.TempPoints[0] != PointF.Empty)
                {
                    int radius;
                    try { radius = Convert.ToInt32(args.Parameters[0]); }
                    catch (Exception) { args.Player.SendMessage("The radius needs to be an integer.", System.Drawing.Color.Red); return; }
                    string theString = "";
                    for (int i = 1; i < args.Parameters.Count; i++)
                    {

                        if (i + 1 != args.Parameters.Count)
                        {
                            theString += args.Parameters[i] + " ";
                        }
                        else
                        {
                            theString += args.Parameters[i];
                        }

                    }
                    bool isWall = false;
                    byte tileType = findTileType(theString);
                    byte wallType = findWallType(theString);
                    isWall = ((tileType == 255) && (wallType != 255));
                    if ((tileType != 255) || (wallType != 255))
                    {
                        int x = args.Player.TempPoints[0].X;
                        int y = args.Player.TempPoints[0].Y;
                        for (int y2 = -radius; y2 <= radius; y2++)
                        {

                            for (int x2 = -radius; x2 <= radius; x2++)
                            {

                                if (Math.Sqrt(Math.Pow(x2, 2) + Math.Pow(y2, 2)) <= radius)
                                {

                                    if (!isWall)
                                    {
                                        changeTile(x + x2, y + y2, tileType, 255);
                                    }
                                    else
                                    {
                                        changeTile(x + x2, y + y2, 255, wallType);
                                    }

                                }

                            }

                        }
                        for (int y2 = -radius; y2 <= radius; y2++)
                        {

                            for (int x2 = -radius; x2 <= radius; x2++)
                            {

                                updateTile(x + x2, y + y2);

                            }

                        }
                        lastArea1[args.Player.Index] = args.Player.TempPoints[0];
                        lastArea2[args.Player.Index] = args.Player.TempPoints[1];
                        args.Player.TempPoints[0] = System.Drawing.Point.Empty;
                        args.Player.TempPoints[1] = System.Drawing.Point.Empty;
                        args.Player.SendMessage("Tiles changed!");
                    }
                    else
                    {

                        args.Player.SendMessage("That is not a recognized tile type.", System.Drawing.Color.Red);

                    }
                }
                else
                {
                    args.Player.SendMessage("Point 1 not set up yet", System.Drawing.Color.Red);
                }
            }
            else
            {

                args.Player.SendMessage("Improper syntax! Proper syntax: /circle radius blocktype", System.Drawing.Color.Red);

            }

        }

        public static void CircleOutline(CommandArgs args)
        {

            if (args.Parameters.Count > 1)
            {
                if (args.Player.TempPoints[0] != PointF.Empty)
                {
                    int radius;
                    try { radius = Convert.ToInt32(args.Parameters[0]); }
                    catch (Exception) { args.Player.SendMessage("The radius needs to be an integer.", System.Drawing.Color.Red); return; }
                    string theString = "";
                    for (int i = 1; i < args.Parameters.Count; i++)
                    {

                        if (i + 1 != args.Parameters.Count)
                        {
                            theString += args.Parameters[i] + " ";
                        }
                        else
                        {
                            theString += args.Parameters[i];
                        }

                    }
                    bool isWall = false;
                    byte tileType = findTileType(theString);
                    byte wallType = findWallType(theString);
                    isWall = ((tileType == 255) && (wallType != 255));
                    if ((tileType != 255) || (wallType != 255))
                    {
                        int x = args.Player.TempPoints[0].X;
                        int y = args.Player.TempPoints[0].Y;
                        for (int y2 = -radius - (int)(brushStroke[args.Player.Index] / 2); y2 <= radius + (int)(brushStroke[args.Player.Index] / 2); y2++)
                        {

                            for (int x2 = -radius - (int)(brushStroke[args.Player.Index] / 2); x2 <= radius + (int)(brushStroke[args.Player.Index] / 2); x2++)
                            {

                                if ((Math.Sqrt(Math.Pow(x2, 2) + Math.Pow(y2, 2)) <= radius + brushStroke[args.Player.Index] / 2) && (Math.Sqrt(Math.Pow(x2, 2) + Math.Pow(y2, 2)) >= radius - brushStroke[args.Player.Index] / 2))
                                {

                                    if (!isWall)
                                    {
                                        changeTile(x + x2, y + y2, tileType, 255);
                                    }
                                    else
                                    {
                                        changeTile(x + x2, y + y2, 255, wallType);
                                    }

                                }

                            }

                        }
                        for (int y2 = -radius - (int)(brushStroke[args.Player.Index] / 2); y2 <= radius + (int)(brushStroke[args.Player.Index] / 2); y2++)
                        {

                            for (int x2 = -radius - (int)(brushStroke[args.Player.Index] / 2); x2 <= radius + (int)(brushStroke[args.Player.Index] / 2); x2++)
                            {

                                updateTile(x + x2, y + y2);

                            }

                        }
                        lastArea1[args.Player.Index] = args.Player.TempPoints[0];
                        lastArea2[args.Player.Index] = args.Player.TempPoints[1];
                        args.Player.TempPoints[0] = System.Drawing.Point.Empty;
                        args.Player.TempPoints[1] = System.Drawing.Point.Empty;
                        args.Player.SendMessage("Tiles changed!");
                    }
                    else
                    {

                        args.Player.SendMessage("That is not a recognized tile type.", System.Drawing.Color.Red);

                    }
                }
                else
                {
                    args.Player.SendMessage("Point 1 not set up yet", System.Drawing.Color.Red);
                }
            }
            else
            {

                args.Player.SendMessage("Improper syntax! Proper syntax: /circleoutline radius blocktype", System.Drawing.Color.Red);

            }

        }

        public static void SemiCircle(CommandArgs args)
        {

            if (args.Parameters.Count > 2)
            {
                if (args.Player.TempPoints[0] != PointF.Empty)
                {
                    int radius;
                    try { radius = Convert.ToInt32(args.Parameters[1]); }
                    catch (Exception) { args.Player.SendMessage("The radius needs to be an integer.", System.Drawing.Color.Red); return; }
                    string theString = "";
                    for (int i = 2; i < args.Parameters.Count; i++)
                    {

                        if (i + 1 != args.Parameters.Count)
                        {
                            theString += args.Parameters[i] + " ";
                        }
                        else
                        {
                            theString += args.Parameters[i];
                        }

                    }
                    bool isWall = false;
                    byte tileType = findTileType(theString);
                    byte wallType = findWallType(theString);
                    isWall = ((tileType == 255) && (wallType != 255));
                    if ((tileType != 255) || (wallType != 255))
                    {
                        int x = args.Player.TempPoints[0].X;
                        int y = args.Player.TempPoints[0].Y;
                        switch (args.Parameters[0].ToLower())
                        {

                            case "left": for (int y2 = -radius; y2 <= radius; y2++)
                                {

                                    for (int x2 = -radius; x2 <= 0; x2++)
                                    {

                                        if (Math.Sqrt(Math.Pow(x2, 2) + Math.Pow(y2, 2)) <= radius)
                                        {

                                            if (!isWall)
                                            {
                                                changeTile(x + x2, y + y2, tileType, 255);
                                            }
                                            else
                                            {
                                                changeTile(x + x2, y + y2, 255, wallType);
                                            }

                                        }

                                    }

                                } break;
                            case "right": for (int y2 = -radius; y2 <= radius; y2++)
                                {

                                    for (int x2 = 0; x2 <= radius; x2++)
                                    {

                                        if (Math.Sqrt(Math.Pow(x2, 2) + Math.Pow(y2, 2)) <= radius)
                                        {

                                            if (!isWall)
                                            {
                                                changeTile(x + x2, y + y2, tileType, 255);
                                            }
                                            else
                                            {
                                                changeTile(x + x2, y + y2, 255, wallType);
                                            }

                                        }

                                    }

                                } break;
                            case "top": for (int y2 = -radius; y2 <= 0; y2++)
                                {

                                    for (int x2 = -radius; x2 <= radius; x2++)
                                    {

                                        if (Math.Sqrt(Math.Pow(x2, 2) + Math.Pow(y2, 2)) <= radius)
                                        {

                                            if (!isWall)
                                            {
                                                changeTile(x + x2, y + y2, tileType, 255);
                                            }
                                            else
                                            {
                                                changeTile(x + x2, y + y2, 255, wallType);
                                            }

                                        }

                                    }

                                } break;
                            case "bottom": for (int y2 = 0; y2 <= radius; y2++)
                                {

                                    for (int x2 = -radius; x2 <= radius; x2++)
                                    {

                                        if (Math.Sqrt(Math.Pow(x2, 2) + Math.Pow(y2, 2)) <= radius)
                                        {

                                            if (!isWall)
                                            {
                                                changeTile(x + x2, y + y2, tileType, 255);
                                            }
                                            else
                                            {
                                                changeTile(x + x2, y + y2, 255, wallType);
                                            }

                                        }

                                    }

                                } break;
                            default: args.Player.SendMessage("Improper syntax! Proper syntax: /semicircle top|left|bottom|right radius blocktype", System.Drawing.Color.Red); return;

                        }
                        for (int y2 = -radius; y2 <= radius; y2++)
                        {

                            for (int x2 = -radius; x2 <= radius; x2++)
                            {

                                updateTile(x + x2, y + y2);

                            }

                        }
                        lastArea1[args.Player.Index] = args.Player.TempPoints[0];
                        lastArea2[args.Player.Index] = args.Player.TempPoints[1];
                        args.Player.TempPoints[0] = System.Drawing.Point.Empty;
                        args.Player.TempPoints[1] = System.Drawing.Point.Empty;
                        args.Player.SendMessage("Tiles changed!");
                    }
                    else
                    {

                        args.Player.SendMessage("That is not a recognized tile type.", System.Drawing.Color.Red);

                    }
                }
                else
                {
                    args.Player.SendMessage("Point 1 not set up yet", System.Drawing.Color.Red);
                }
            }
            else
            {

                args.Player.SendMessage("Improper syntax! Proper syntax: /semicircle top|left|bottom|right radius blocktype", System.Drawing.Color.Red);

            }

        }

        public static void SemiCircleOutline(CommandArgs args)
        {

            if (args.Parameters.Count > 2)
            {
                if (args.Player.TempPoints[0] != PointF.Empty)
                {
                    int radius;
                    try { radius = Convert.ToInt32(args.Parameters[1]); }
                    catch (Exception) { args.Player.SendMessage("The radius needs to be an integer.", System.Drawing.Color.Red); return; }
                    string theString = "";
                    for (int i = 2; i < args.Parameters.Count; i++)
                    {

                        if (i + 1 != args.Parameters.Count)
                        {
                            theString += args.Parameters[i] + " ";
                        }
                        else
                        {
                            theString += args.Parameters[i];
                        }

                    }
                    bool isWall = false;
                    byte tileType = findTileType(theString);
                    byte wallType = findWallType(theString);
                    isWall = ((tileType == 255) && (wallType != 255));
                    if ((tileType != 255) || (wallType != 255))
                    {
                        int x = args.Player.TempPoints[0].X;
                        int y = args.Player.TempPoints[0].Y;
                        switch (args.Parameters[0].ToLower())
                        {

                            case "left": for (int y2 = -radius - (int)(brushStroke[args.Player.Index] / 2); y2 <= radius + (int)(brushStroke[args.Player.Index] / 2); y2++)
                                {

                                    for (int x2 = -radius - (int)(brushStroke[args.Player.Index] / 2); x2 <= 0; x2++)
                                    {

                                        if ((Math.Sqrt(Math.Pow(x2, 2) + Math.Pow(y2, 2)) <= radius + brushStroke[args.Player.Index] / 2) && (Math.Sqrt(Math.Pow(x2, 2) + Math.Pow(y2, 2)) >= radius - brushStroke[args.Player.Index] / 2))
                                        {

                                            if (!isWall)
                                            {
                                                changeTile(x + x2, y + y2, tileType, 255);
                                            }
                                            else
                                            {
                                                changeTile(x + x2, y + y2, 255, wallType);
                                            }

                                        }

                                    }

                                } break;
                            case "right": for (int y2 = -radius - (int)(brushStroke[args.Player.Index] / 2); y2 <= radius + (int)(brushStroke[args.Player.Index] / 2); y2++)
                                {

                                    for (int x2 = 0; x2 <= radius + (int)(brushStroke[args.Player.Index] / 2); x2++)
                                    {

                                        if ((Math.Sqrt(Math.Pow(x2, 2) + Math.Pow(y2, 2)) <= radius + brushStroke[args.Player.Index] / 2) && (Math.Sqrt(Math.Pow(x2, 2) + Math.Pow(y2, 2)) >= radius - brushStroke[args.Player.Index] / 2))
                                        {

                                            if (!isWall)
                                            {
                                                changeTile(x + x2, y + y2, tileType, 255);
                                            }
                                            else
                                            {
                                                changeTile(x + x2, y + y2, 255, wallType);
                                            }

                                        }

                                    }

                                } break;
                            case "top": for (int y2 = -radius - (int)(brushStroke[args.Player.Index] / 2); y2 <= 0; y2++)
                                {

                                    for (int x2 = -radius - (int)(brushStroke[args.Player.Index] / 2); x2 <= radius + (int)(brushStroke[args.Player.Index] / 2); x2++)
                                    {

                                        if ((Math.Sqrt(Math.Pow(x2, 2) + Math.Pow(y2, 2)) <= radius + brushStroke[args.Player.Index] / 2) && (Math.Sqrt(Math.Pow(x2, 2) + Math.Pow(y2, 2)) >= radius - brushStroke[args.Player.Index] / 2))
                                        {

                                            if (!isWall)
                                            {
                                                changeTile(x + x2, y + y2, tileType, 255);
                                            }
                                            else
                                            {
                                                changeTile(x + x2, y + y2, 255, wallType);
                                            }

                                        }

                                    }

                                } break;
                            case "bottom": for (int y2 = 0; y2 <= radius + (int)(brushStroke[args.Player.Index] / 2); y2++)
                                {

                                    for (int x2 = -radius - (int)(brushStroke[args.Player.Index] / 2); x2 <= radius + (int)(brushStroke[args.Player.Index] / 2); x2++)
                                    {

                                        if ((Math.Sqrt(Math.Pow(x2, 2) + Math.Pow(y2, 2)) <= radius + brushStroke[args.Player.Index] / 2) && (Math.Sqrt(Math.Pow(x2, 2) + Math.Pow(y2, 2)) >= radius - brushStroke[args.Player.Index] / 2))
                                        {

                                            if (!isWall)
                                            {
                                                changeTile(x + x2, y + y2, tileType, 255);
                                            }
                                            else
                                            {
                                                changeTile(x + x2, y + y2, 255, wallType);
                                            }

                                        }

                                    }

                                } break;
                            default: args.Player.SendMessage("Improper syntax! Proper syntax: /semicircleoutline top|left|bottom|right radius blocktype", System.Drawing.Color.Red); return;

                        }
                        for (int y2 = -radius - (int)(brushStroke[args.Player.Index] / 2); y2 <= radius + (int)(brushStroke[args.Player.Index] / 2); y2++)
                        {

                            for (int x2 = -radius - (int)(brushStroke[args.Player.Index] / 2); x2 <= radius + (int)(brushStroke[args.Player.Index] / 2); x2++)
                            {

                                updateTile(x + x2, y + y2);

                            }

                        }
                        lastArea1[args.Player.Index] = args.Player.TempPoints[0];
                        lastArea2[args.Player.Index] = args.Player.TempPoints[1];
                        args.Player.TempPoints[0] = System.Drawing.Point.Empty;
                        args.Player.TempPoints[1] = System.Drawing.Point.Empty;
                        args.Player.SendMessage("Tiles changed!");
                    }
                    else
                    {

                        args.Player.SendMessage("That is not a recognized tile type.", System.Drawing.Color.Red);

                    }
                }
                else
                {
                    args.Player.SendMessage("Point 1 not set up yet", System.Drawing.Color.Red);
                }
            }
            else
            {

                args.Player.SendMessage("Improper syntax! Proper syntax: /semicircleoutline top|left|bottom|right radius blocktype", System.Drawing.Color.Red);

            }

        }

        public static void changeTile(int x, int y, byte type, byte wall)
        {

            if (type < 253)
            {

                Main.tile[x, y].type = type;
                Main.tile[x, y].active = true;
                Main.tile[x, y].liquid = 0;
                Main.tile[x, y].skipLiquid = true;
                Main.tile[x, y].frameNumber = 0;
                Main.tile[x, y].frameX = -1;
                Main.tile[x, y].frameY = -1;

            }
            else if (type == 253)
            {

                Main.tile[x, y].active = false;
                Main.tile[x, y].skipLiquid = false;
                Main.tile[x, y].lava = false;
                Main.tile[x, y].liquid = 255;
                Main.tile[x, y].checkingLiquid = false;

            }
            else if (type == 254)
            {

                Main.tile[x, y].active = false;
                Main.tile[x, y].skipLiquid = false;
                Main.tile[x, y].lava = true;
                Main.tile[x, y].liquid = 255;
                Main.tile[x, y].checkingLiquid = false;

            }
            if (wall < 255)
            {

                Main.tile[x, y].wall = wall;

            }
            if ((Main.tile[x, y].type == 53) || (Main.tile[x, y].type == 253) || (Main.tile[x, y].type == 254))
            WorldGen.SquareTileFrame(x, y, false);

        }

        public static byte findTileType(string theString)
        {

            theString = theString.ToLower();
            List<byte> onesThatMatch = new List<byte>();
            foreach (KeyValuePair<string, byte> entry in tileTypeNames)
            {

                if (theString != entry.Key)
                {

                    if (entry.Key.StartsWith(theString))
                    {

                        onesThatMatch.Add(entry.Value);

                    }

                }
                else
                {

                    return (entry.Value);

                }

            }
            if (onesThatMatch.Count == 1)
            {

                return (onesThatMatch[0]);

            }
            else
            {

                return (255);

            }

        }

        public static byte findWallType(string theString)
        {

            theString = theString.ToLower();
            List<byte> onesThatMatch = new List<byte>();
            foreach (KeyValuePair<string, byte> entry in wallTypeNames)
            {

                if (theString != entry.Key)
                {

                    if (entry.Key.StartsWith(theString))
                    {

                        onesThatMatch.Add(entry.Value);

                    }

                }
                else
                {

                    return (entry.Value);

                }

            }
            if (onesThatMatch.Count == 1)
            {

                return (onesThatMatch[0]);

            }
            else
            {

                return (255);

            }

        }
        public static byte findTileByID(int tileID)
        {

            foreach (KeyValuePair<string, byte> entry in tileTypeNames)
            {

                if (tileID == entry.Value)
                {

                    return (entry.Value);

                }

            }
            return (255);

        }

        public static bool rectCollision(int x1, int y1, int x2, int y2, int x3, int y3, int x4, int y4)
        {

            if ((x1 >= x3) && (x1 <= x4) && (y1 >= y3) && (y1 <= y4)) return (true);
            if ((x2 >= x3) && (x2 <= x4) && (y2 >= y3) && (y2 <= y4)) return (true);
            if ((x1 >= x3) && (x1 <= x4) && (y2 >= y3) && (y2 <= y4)) return (true);
            if ((x2 >= x3) && (x2 <= x4) && (y1 >= y3) && (y1 <= y4)) return (true);
            return (false);

        }

        public static void updateTile(int x, int y)
        {

            x = Netplay.GetSectionX(x);
            y = Netplay.GetSectionY(y);
            for (int i = 0; i < Netplay.serverSock.Length; i++)
            {
            Netplay.serverSock[0].tileSection[x, y] = false;
            }


        }
    }
}