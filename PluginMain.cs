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
        public static int[] copyX = new int[256];
        public static int[] copyY = new int[256];
        public static int[] copyW = new int[256];
        public static int[] copyH = new int[256];
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
        }
        public override void DeInitialize()
        {
            GameHooks.Initialize -= OnInitialize;
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
            tileTypeNames.Add("diamond",68);
            tileTypeNames.Add("obsidian brick",75);
            tileTypeNames.Add("hellstone brick",76);
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

            Commands.ChatCommands.Add(new Command("tilepasting", ClearArea, "cleararea"));
            Commands.ChatCommands.Add(new Command("tilepasting", Rectangle, "rectangle"));
            Commands.ChatCommands.Add(new Command("tilepasting", RectangleOutline, "rectangleoutline"));
            Commands.ChatCommands.Add(new Command("tilepasting", Circle, "oval"));
            Commands.ChatCommands.Add(new Command("tilepasting", CircleOutline, "ovaloutline"));
            Commands.ChatCommands.Add(new Command("tilepasting", SemiCircle, "semioval"));
            //Commands.ChatCommands.Add(new Command("tilepasting", Rectangle, "semiovaloutline"));
            Commands.ChatCommands.Add(new Command("tilepasting", Copy, "copy"));
            Commands.ChatCommands.Add(new Command("tilepasting", Paste, "paste"));
        }

        public static void Circle(CommandArgs args)
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

                                TSPlayer.All.SendTileSquare(x + x2, y + y2, 3);

                            }

                        }
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

        public static void SemiCircle(CommandArgs args)
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

                                }

                            }

                        }
                        for (int y2 = 0; y2 <= height; y2++)
                        {

                            for (int x2 = 0; x2 <= width; x2++)
                            {

                                TSPlayer.All.SendTileSquare(x + x2, y + y2, 3);

                            }

                        }
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

        public static void CircleOutline(CommandArgs args)
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

                                TSPlayer.All.SendTileSquare(x + x2, y + y2, 3);

                            }

                        }
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

                        args.Player.SendMessage("You cannot paste into the copied area.", System.Drawing.Color.Red);
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
                                Main.tile[x + x2, y + y2].active = Main.tile[X,Y].active;
                                try { Main.tile[x + x2, y + y2].type = Main.tile[X, Y].type; }
                                catch (NullReferenceException) { Main.tile[x + x2, y + y2].active = false; }
                                try { Main.tile[x + x2, y + y2].wall = Main.tile[X, Y].wall; }
                                catch (NullReferenceException) { Main.tile[x + x2, y + y2].wall = 0; }
                                try { Main.tile[x + x2, y + y2].frameNumber = Main.tile[X, Y].frameNumber; }
                                catch (NullReferenceException) { Main.tile[x + x2, y + y2].frameNumber = 1; }
                                try { Main.tile[x + x2, y + y2].frameX = Main.tile[X, Y].frameX; }
                                catch (NullReferenceException) { Main.tile[x + x2, y + y2].frameX = 1; }
                                try { Main.tile[x + x2, y + y2].frameY = Main.tile[X, Y].frameY; }
                                catch (NullReferenceException) { Main.tile[x + x2, y + y2].frameY = 1; }
                                Main.tile[x + x2, y + y2].checkingLiquid = Main.tile[X, Y].checkingLiquid;
                                Main.tile[x + x2, y + y2].lava = Main.tile[X, Y].lava;
                                try { Main.tile[x + x2, y + y2].liquid = Main.tile[X, Y].liquid; }
                                catch (NullReferenceException) { Main.tile[x + x2, y + y2].liquid = 0; }
                                Main.tile[x + x2, y + y2].skipLiquid = Main.tile[X, Y].skipLiquid;
                            }
                            catch (Exception) { }

                        }

                    }
                    for (int y2 = 0; y2 <= height; y2++)
                    {

                        for (int x2 = 0; x2 <= width; x2++)
                        {

                            try
                            {
                                TSPlayer.All.SendTileSquare(x + x2, y + y2, 3);
                            }
                            catch (Exception) { }

                        }

                    }
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

            if (!args.Player.TempPoints.Any(p => p == PointF.Empty))
            {

                copyX[args.Player.Index] = Math.Min(args.Player.TempPoints[0].X, args.Player.TempPoints[1].X);
                copyY[args.Player.Index] = Math.Min(args.Player.TempPoints[0].Y, args.Player.TempPoints[1].Y);
                copyW[args.Player.Index] = Math.Abs(args.Player.TempPoints[0].X - args.Player.TempPoints[1].X);
                copyH[args.Player.Index] = Math.Abs(args.Player.TempPoints[0].Y - args.Player.TempPoints[1].Y);
                args.Player.TempPoints[0] = System.Drawing.Point.Empty;
                args.Player.TempPoints[1] = System.Drawing.Point.Empty;
                args.Player.SendMessage("You have successfully copied to the clipboard.");
            }
            else
            {
                args.Player.SendMessage("Points not set up yet", System.Drawing.Color.Red);
            }

        }

        public static void ClearArea(CommandArgs args)
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

                        Main.tile[args.Player.TempPoints[0].X + x2, args.Player.TempPoints[0].Y + y2].active = false;
                        Main.tile[args.Player.TempPoints[0].X + x2, args.Player.TempPoints[0].Y + y2].wall = 0;

                    }

                }
                for (int y2 = 0; y2 <= height; y2++)
                {

                    for (int x2 = 0; x2 <= width; x2++)
                    {

                        TSPlayer.All.SendTileSquare(x + x2, y + y2, 3);

                    }

                }
                args.Player.TempPoints[0] = System.Drawing.Point.Empty;
                args.Player.TempPoints[1] = System.Drawing.Point.Empty;
                args.Player.SendMessage("Tiles cleared!");
            }
            else
            {
                args.Player.SendMessage("Points not set up yet", System.Drawing.Color.Red);
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

                                TSPlayer.All.SendTileSquare(x + x2, y + y2, 3);

                            }

                        }
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
                        for (int y2 = 0; y2 <= height; y2++)
                        {

                            for (int x2 = 0; x2 <= width; x2++)
                            {

                                if ((x2 == 0) || (y2 == 0) || (x2 == width) || (y2 == height))
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

                                TSPlayer.All.SendTileSquare(x + x2, y + y2, 3);

                            }

                        }
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

        public static void changeTile(int x, int y, byte type, byte wall)
        {

            if (type < 255)
            {

                Main.tile[x, y].type = type;
                Main.tile[x, y].active = true;

            }
            Main.tile[x, y].frameNumber = 1;
            Main.tile[x, y].frameX = 1;
            Main.tile[x, y].frameY = 1;
            if (wall < 255)
            {

                Main.tile[x, y].wall = wall;

            }

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

        public static bool rectCollision(int x1, int y1, int x2, int y2, int x3, int y3, int x4, int y4)
        {

            if ((x1 >= x3) && (x1 <= x4) && (y1 >= y3) && (y1 <= y4)) return (true);
            if ((x2 >= x3) && (x2 <= x4) && (y2 >= y3) && (y2 <= y4)) return (true);
            if ((x1 >= x3) && (x1 <= x4) && (y2 >= y3) && (y2 <= y4)) return (true);
            if ((x2 >= x3) && (x2 <= x4) && (y1 >= y3) && (y1 <= y4)) return (true);
            return (false);

        }
    }
}