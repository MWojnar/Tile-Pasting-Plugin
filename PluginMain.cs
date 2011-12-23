using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Drawing;
using Community.CsharpSqlite.SQLiteClient;
using MySql.Data.MySqlClient;
using Microsoft.Xna.Framework;
using Terraria;
using Hooks;
using TShockAPI;
using TShockAPI.DB;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Threading;

namespace TileEditing
{
    [APIVersion(1, 10)]
    public class TileEditing : TerrariaPlugin
    {
        public static Dictionary<string, byte> tileTypeNames = new Dictionary<string, byte>();
        public static Dictionary<string, byte> wallTypeNames = new Dictionary<string, byte>();
        public static double[] brushStroke = new double[256];
        public static int[] copyX = new int[256];
        public static int[] copyY = new int[256];
        public static int[] copyW = new int[256];
        public static int[] copyH = new int[256];
        public static int[] clipboardType = new int[256];
        public static Dictionary<Point, Tile> selectTiles = new Dictionary<Point, Tile>(1000);
        public static bool[] cut = new bool[256];
        public static List<Point>[] lastAreas = new List<Point>[256];
        public static List<Point>[] tempPoints = new List<Point>[256];
        public static int[] awaitingPoint = new int[256];
        public static Dictionary<Point, TileData> tilesToRevert = new Dictionary<Point, TileData>();
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
            NetHooks.GetData += OnGetData;
            NetHooks.SendData += OnSendData;
            GameHooks.Update += OnUpdate;
        }
        protected override void  Dispose(bool disposing)
        {
            if (disposing)
            {
                GameHooks.Initialize -= OnInitialize;
                ServerHooks.Leave -= OnLeave;
                NetHooks.GetData -= OnGetData;
                NetHooks.SendData -= OnSendData;
                GameHooks.Update -= OnUpdate;
            }
        }
        public TileEditing(Main game)
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
                cut[i] = false;
                tempPoints[i] = new List<Point>();
                awaitingPoint[i] = 0;

            }
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
            tileTypeNames.Add("red brick",39);
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
            tileTypeNames.Add("cobalt ore", 107);
            tileTypeNames.Add("mythril ore", 108);
            tileTypeNames.Add("hallowed grass", 109);
            tileTypeNames.Add("adamantite ore", 111);
            tileTypeNames.Add("ebonsand", 112);
            tileTypeNames.Add("pearlsand", 116);
            tileTypeNames.Add("pearlstone", 117);
            tileTypeNames.Add("pearlstone brick", 118);
            tileTypeNames.Add("iridescent brick", 119);
            tileTypeNames.Add("mudstone brick", 120);
            tileTypeNames.Add("cobalt brick", 121);
            tileTypeNames.Add("mythril brick", 122);
            tileTypeNames.Add("silt", 123);
            tileTypeNames.Add("wooden beam", 124);
            tileTypeNames.Add("ice", 127);
            tileTypeNames.Add("active stone block", 130);
            tileTypeNames.Add("inactive stone block", 131);
            tileTypeNames.Add("dart trap", 137);
            tileTypeNames.Add("demonite brick", 140);
            tileTypeNames.Add("explosives", 141);
            tileTypeNames.Add("inlet pump", 142);
            tileTypeNames.Add("outlet pump", 143);
            tileTypeNames.Add("air", 250);
            tileTypeNames.Add("air back", 251);
            tileTypeNames.Add("air front", 252);
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
            wallTypeNames.Add("glass wall", 21);
            wallTypeNames.Add("pearlstone brick wall", 22);
            wallTypeNames.Add("iridescent brick wall", 23);
            wallTypeNames.Add("mudstone brick wall", 24);
            wallTypeNames.Add("cobalt brick wall", 25);
            wallTypeNames.Add("mythril brick wall", 26);
            wallTypeNames.Add("planked wall", 27);
            wallTypeNames.Add("pearlstone wall", 28);
            List<string> permlist = new List<string>();
            permlist.Add("tilepasting");
            TShock.Groups.AddPermissions("trustedadmin", permlist);

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
            Commands.ChatCommands.Add(new Command("tilepasting", BrushStroke, "brush"));
            Commands.ChatCommands.Add(new Command("tilepasting", LastArea, "lastarea"));
            //Commands.ChatCommands.Add(new Command("tilepasting", Undo, "undo"));
            Commands.ChatCommands.Add(new Command("tilepasting", Tiles, "tile"));
            //Commands.ChatCommands.Add(new Command("tilepasting", Triangle, "triangle"));
            Commands.ChatCommands.Add(new Command("tilepasting", TriangleOutline, "triangleoutline"));
            Commands.ChatCommands.Add(new Command("tilepasting", Hallow, "hallow"));
            Commands.ChatCommands.Add(new Command("tilepasting", Corrupt, "corrupt"));
            Commands.ChatCommands.Add(new Command("tilepasting", NormalLand, "normalland"));
        }

        public void Hallow(CommandArgs args)
        {

            if (tempPoints[args.Player.Index].Count() > 1)
            {

                        int x = Math.Min(tempPoints[args.Player.Index][0].X, tempPoints[args.Player.Index][1].X);
                        int y = Math.Min(tempPoints[args.Player.Index][0].Y, tempPoints[args.Player.Index][1].Y);
                        int width = Math.Abs(tempPoints[args.Player.Index][0].X - tempPoints[args.Player.Index][1].X);
                        int height = Math.Abs(tempPoints[args.Player.Index][0].Y - tempPoints[args.Player.Index][1].Y);
                        for (int y2 = height; y2 >= 0; y2--)
                        {

                            for (int x2 = 0; x2 <= width; x2++)
                            {

                                switch (Main.tile[x + x2, y + y2].type)
                                {
                                    
                                    case 1: Main.tile[x + x2, y + y2].type = 117; break;
                                    case 2: Main.tile[x + x2, y + y2].type = 109; break;
                                    case 3: Main.tile[x + x2, y + y2].type = 110; break;
                                    case 23: Main.tile[x + x2, y + y2].type = 109; break;
                                    case 24: Main.tile[x + x2, y + y2].type = 110; break;
                                    case 25: Main.tile[x + x2, y + y2].type = 117; break;
                                    case 26: changeTile(x + x2, y + y2, 252, 255); break;
                                    case 31: changeTile(x + x2, y + y2, 252, 255); break;
                                    case 32: changeTile(x + x2, y + y2, 252, 255); break;
                                    case 52: Main.tile[x + x2, y + y2].type = 115; break;
                                    case 53: Main.tile[x + x2, y + y2].type = 116; break;
                                    case 73: changeTile(x + x2, y + y2, 252, 255); break;
                                    case 112: Main.tile[x + x2, y + y2].type = 116; break;
                                    case 119: Main.tile[x + x2, y + y2].type = 118; break;

                                }

                                if (Main.tile[x + x2, y + y2].wall == 1 || Main.tile[x + x2, y + y2].wall == 3)
                                    Main.tile[x + x2, y + y2].wall = 28;

                            }
                            
                        }
                        for (int y2 = height; y2 >= 0; y2--)
                        {

                            for (int x2 = 0; x2 <= width; x2++)
                            {

                                updateTile(x + x2, y + y2);

                            }

                        }
                        lastAreas[args.Player.Index] = tempPoints[args.Player.Index];
                        clearTempPoints((byte)args.Player.Index);

                        args.Player.SendMessage("Tiles changed!");

            }
            else
            {
                args.Player.SendMessage("Points not set up yet", Color.Red);
            }

        }

        public void Corrupt(CommandArgs args)
        {

            if (tempPoints[args.Player.Index].Count() > 1)
            {

                int x = Math.Min(tempPoints[args.Player.Index][0].X, tempPoints[args.Player.Index][1].X);
                int y = Math.Min(tempPoints[args.Player.Index][0].Y, tempPoints[args.Player.Index][1].Y);
                int width = Math.Abs(tempPoints[args.Player.Index][0].X - tempPoints[args.Player.Index][1].X);
                int height = Math.Abs(tempPoints[args.Player.Index][0].Y - tempPoints[args.Player.Index][1].Y);
                for (int y2 = height; y2 >= 0; y2--)
                {

                    for (int x2 = 0; x2 <= width; x2++)
                    {

                        switch (Main.tile[x + x2, y + y2].type)
                        {

                            case 1: Main.tile[x + x2, y + y2].type = 25; break;
                            case 2: Main.tile[x + x2, y + y2].type = 23; break;
                            case 3: Main.tile[x + x2, y + y2].type = 24; break;
                            case 109: Main.tile[x + x2, y + y2].type = 23; break;
                            case 110: Main.tile[x + x2, y + y2].type = 24; break;
                            case 117: Main.tile[x + x2, y + y2].type = 25; break;
                            case 52: changeTile(x + x2, y + y2, 252, 255); break;
                            case 53: Main.tile[x + x2, y + y2].type = 112; break;
                            case 73: changeTile(x + x2, y + y2, 252, 255); break;
                            case 113: changeTile(x + x2, y + y2, 252, 255); break;
                            case 115: changeTile(x + x2, y + y2, 252, 255); break;
                            case 116: Main.tile[x + x2, y + y2].type = 112; break;
                            case 118: Main.tile[x + x2, y + y2].type = 119; break;

                        }

                        if (Main.tile[x + x2, y + y2].wall == 1 || Main.tile[x + x2, y + y2].wall == 28)
                            Main.tile[x + x2, y + y2].wall = 3;

                    }

                }
                for (int y2 = height; y2 >= 0; y2--)
                {

                    for (int x2 = 0; x2 <= width; x2++)
                    {

                        updateTile(x + x2, y + y2);

                    }

                }
                lastAreas[args.Player.Index] = tempPoints[args.Player.Index];
                clearTempPoints((byte)args.Player.Index);

                args.Player.SendMessage("Tiles changed!");

            }
            else
            {
                args.Player.SendMessage("Points not set up yet", Color.Red);
            }

        }

        public void NormalLand(CommandArgs args)
        {

            if (tempPoints[args.Player.Index].Count() > 1)
            {

                int x = Math.Min(tempPoints[args.Player.Index][0].X, tempPoints[args.Player.Index][1].X);
                int y = Math.Min(tempPoints[args.Player.Index][0].Y, tempPoints[args.Player.Index][1].Y);
                int width = Math.Abs(tempPoints[args.Player.Index][0].X - tempPoints[args.Player.Index][1].X);
                int height = Math.Abs(tempPoints[args.Player.Index][0].Y - tempPoints[args.Player.Index][1].Y);
                for (int y2 = height; y2 >= 0; y2--)
                {

                    for (int x2 = 0; x2 <= width; x2++)
                    {

                        switch (Main.tile[x + x2, y + y2].type)
                        {

                            case 117: Main.tile[x + x2, y + y2].type = 1; break;
                            case 109: Main.tile[x + x2, y + y2].type = 2; break;
                            case 110: Main.tile[x + x2, y + y2].type = 3; break;
                            case 23: Main.tile[x + x2, y + y2].type = 2; break;
                            case 24: Main.tile[x + x2, y + y2].type = 3; break;
                            case 25: Main.tile[x + x2, y + y2].type = 1; break;
                            case 26: changeTile(x + x2, y + y2, 252, 255); break;
                            case 31: changeTile(x + x2, y + y2, 252, 255); break;
                            case 32: changeTile(x + x2, y + y2, 252, 255); break;
                            case 52: Main.tile[x + x2, y + y2].type = 115; break;
                            case 112: Main.tile[x + x2, y + y2].type = 53; break;
                            case 113: changeTile(x + x2, y + y2, 252, 255); break;
                            case 116: Main.tile[x + x2, y + y2].type = 53; break;

                        }

                        if (Main.tile[x + x2, y + y2].wall == 3 || Main.tile[x + x2, y + y2].wall == 28)
                            Main.tile[x + x2, y + y2].wall = 1;

                    }

                }
                for (int y2 = height; y2 >= 0; y2--)
                {

                    for (int x2 = 0; x2 <= width; x2++)
                    {

                        updateTile(x + x2, y + y2);

                    }

                }
                lastAreas[args.Player.Index] = tempPoints[args.Player.Index];
                clearTempPoints((byte)args.Player.Index);

                args.Player.SendMessage("Tiles changed!");

            }
            else
            {
                args.Player.SendMessage("Points not set up yet", Color.Red);
            }

        }

        private void Test(CommandArgs args)
        {

            try
            {
                Main.tile[args.Player.TileX + 5, args.Player.TileY].wall = Convert.ToByte(args.Parameters[0]);
                updateTile(args.Player.TileX + 5, args.Player.TileY);
            }
            catch (Exception) { args.Player.SendMessage("Invalid Number!", Color.Red); }

        }

        private void OnUpdate()
        {

            foreach(KeyValuePair<Point, TileData> entry in tilesToRevert)
            {

                Main.tile[entry.Key.X, entry.Key.Y].type = entry.Value.type;
                Main.tile[entry.Key.X, entry.Key.Y].active = entry.Value.active;

            }
            tilesToRevert.Clear();

        }

        private void OnSendData(SendDataEventArgs e)
        {

            if (e.MsgID == PacketTypes.TileSendSection)
            {

                short width = (short)e.number;
                int x = (int)e.number2;
                int y = (int)e.number3;
                foreach (Point thePoint in getTempOutline(tempPoints[e.remoteClient]))
                {
                    if (!tilesToRevert.Keys.Contains(new Point(thePoint.X, thePoint.Y)))
                    {
                        tilesToRevert.Add(new Point(thePoint.X, thePoint.Y), Main.tile[thePoint.X, thePoint.Y].Data);
                        Main.tile[thePoint.X, thePoint.Y].active = true;
                        Main.tile[thePoint.X, thePoint.Y].type = 70;
                    }
                }

            }
            
        }

        private void OnGetData(GetDataEventArgs e)
        {

            if ((e.MsgID == PacketTypes.Tile) || (e.MsgID == PacketTypes.TileKill))
            {

                if (awaitingPoint[e.Msg.whoAmI] != 0)
                {

                    List<Point> tempTempPoint = getTempOutline(tempPoints[e.Msg.whoAmI]);
                    /*switch (tempPoints[e.Msg.whoAmI].Count())
                    {

                        case 1: updateTile(tempPoints[e.Msg.whoAmI][0].X, tempPoints[e.Msg.whoAmI][0].Y);

                    }*/
                    using (var data = new MemoryStream(e.Msg.readBuffer, e.Index, e.Length))
                    {
                        var reader = new BinaryReader(data);
                        byte action = reader.ReadByte();
                        int tilex = reader.ReadInt32();
                        int tiley = reader.ReadInt32();
                        if (awaitingPoint[e.Msg.whoAmI] <= -1)
                        {

                            tempPoints[e.Msg.whoAmI].Add(new Point(tilex, tiley));
                            TShock.Players[e.Msg.whoAmI].SendMessage("Point #" + tempPoints[e.Msg.whoAmI].Count().ToString() + " set.");

                        }
                        else if (awaitingPoint[e.Msg.whoAmI] - 1 < tempPoints[e.Msg.whoAmI].Count())
                        {

                            try
                            {
                                tempPoints[e.Msg.whoAmI][awaitingPoint[e.Msg.whoAmI] - 1] = new Point(tilex, tiley);
                                TShock.Players[e.Msg.whoAmI].SendMessage("Point #" + awaitingPoint[e.Msg.whoAmI].ToString() + " set.");
                                awaitingPoint[e.Msg.whoAmI] = 0;
                            }
                            catch (Exception)
                            {
                                TShock.Players[e.Msg.whoAmI].SendMessage("There has been an error, please try entering in the /tile set command again.", Color.Red);
                                awaitingPoint[e.Msg.whoAmI] = 0;
                                NetMessage.SendTileSquare(e.Msg.whoAmI, tilex, tiley, 1);
                            }

                        }
                        else if (awaitingPoint[e.Msg.whoAmI] - 1 == tempPoints[e.Msg.whoAmI].Count())
                        {

                            tempPoints[e.Msg.whoAmI].Add(new Point(tilex, tiley));
                            TShock.Players[e.Msg.whoAmI].SendMessage("Point #" + awaitingPoint[e.Msg.whoAmI].ToString() + " set.");
                            awaitingPoint[e.Msg.whoAmI] = 0;

                        }
                        else
                        {

                            TShock.Players[e.Msg.whoAmI].SendMessage("There has been an error, please try entering in the /tile set command again.", Color.Red);
                            awaitingPoint[e.Msg.whoAmI] = 0;
                            NetMessage.SendTileSquare(e.Msg.whoAmI, tilex, tiley, 1);

                        }
                        refreshTempTiles(e.Msg.whoAmI, tempTempPoint);

                    }
                    e.Handled = true;
                }

            }

        }

        private static void OnLeave(int ply)
        {

            copyX[ply] = 0;
            copyY[ply] = 0;
            copyW[ply] = 0;
            copyH[ply] = 0;
            cut[ply] = false;
            brushStroke[ply] = 1.0;
            lastAreas[ply] = new List<Point>();
            tempPoints[ply] = new List<Point>();
            awaitingPoint[ply] = 0;

        }

        public static void TriangleOutline(CommandArgs args)
        {

            if (args.Parameters.Count > 0)
            {
                if (tempPoints[args.Player.Index].Count() > 2)
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
                        int trueX = tempPoints[args.Player.Index][0].X;
                        int trueY = tempPoints[args.Player.Index][0].Y;
                        int trueX2 = tempPoints[args.Player.Index][1].X;
                        int trueY2 = tempPoints[args.Player.Index][1].Y;
                        int trueX3 = tempPoints[args.Player.Index][2].X;
                        int trueY3 = tempPoints[args.Player.Index][2].Y;
                        if (!isWall)
                        {
                            drawLine(trueX, trueY, trueX2, trueY2, tileType, 255, brushStroke[args.Player.Index]);
                            drawLine(trueX3, trueY3, trueX2, trueY2, tileType, 255, brushStroke[args.Player.Index]);
                            drawLine(trueX, trueY, trueX3, trueY3, tileType, 255, brushStroke[args.Player.Index]);
                        }
                        else
                        {
                            drawLine(trueX, trueY, trueX2, trueY2, 255, wallType, brushStroke[args.Player.Index]);
                            drawLine(trueX3, trueY3, trueX2, trueY2, 255, wallType, brushStroke[args.Player.Index]);
                            drawLine(trueX, trueY, trueX3, trueY3, 255, wallType, brushStroke[args.Player.Index]);
                        }
                        lastAreas[args.Player.Index] = tempPoints[args.Player.Index];
                        clearTempPoints((byte)args.Player.Index);

                        args.Player.SendMessage("Tiles changed!");
                    }
                    else
                    {

                        args.Player.SendMessage("That is not a recognized tile type.", Color.Red);

                    }
                }
                else
                {
                    args.Player.SendMessage("Points not set up yet", Color.Red);
                }
            }
            else
            {

                args.Player.SendMessage("Improper syntax! Proper syntax: /triangleoutline blocktype", Color.Red);

            }

        }

        public static void Triangle(CommandArgs args)
        {



        }

        public static void Tiles(CommandArgs args)
        {

            if (args.Parameters.Count() > 0)
            {

                switch (args.Parameters[0].ToLower())
                {

                    case "set": if (args.Parameters.Count() > 1) switch (args.Parameters[1].ToLower())
                            {

                                case "all": awaitingPoint[args.Player.Index] = -1;
                                args.Player.SendMessage("Awaiting multiple new tile points.");
                                break;
                                default: try
                                {

                                    awaitingPoint[args.Player.Index] = Convert.ToInt32(args.Parameters[1]);
                                    args.Player.SendMessage("Awaiting tile point #" + args.Parameters[1]);


                                }
                                catch (Exception) { args.Player.SendMessage("Please use an integer or the keyword \"all\".", Color.Red); }
                                break;

                            } break;
                    case "clear": clearTempPoints((byte)args.Player.Index); args.Player.SendMessage("Tile Points successfully cleared."); break;

                }

            }
            else
            {

                args.Player.SendMessage("Improper Syntax.  Proper Syntax: /tile set all|(number), or /tile clear", Color.Red);

            }

        }

        public static void Undo(CommandArgs args)
        {

            

        }

        public static void LastArea(CommandArgs args)
        {

            if (lastAreas[args.Player.Index].Count() > 0)
            {

                List<Point> tempTempPoint = getTempOutline(tempPoints[args.Player.Index]);
                tempPoints[args.Player.Index] = lastAreas[args.Player.Index];
                refreshTempTiles(args.Player.Index, tempTempPoint);
                args.Player.SendMessage("Tile points reset.");

            }
            else
            {

                args.Player.SendMessage("You haven't set any tile points in the past.", Color.Red);

            }
            lastAreas[args.Player.Index] = new List<Point>();

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
                catch (Exception) { args.Player.SendMessage("You must give us a proper number.", Color.Red); return; }

            }
            else
            {

                args.Player.SendMessage("Improper Syntax. Proper Syntax: /brushstroke size", Color.Red);

            }

        }

        public static void Line(CommandArgs args)
        {

            if (args.Parameters.Count > 0)
            {
                if (tempPoints[args.Player.Index].Count() > 1)
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
                        int x = Math.Min(tempPoints[args.Player.Index][0].X, tempPoints[args.Player.Index][1].X);
                        int y = Math.Min(tempPoints[args.Player.Index][0].Y, tempPoints[args.Player.Index][1].Y);
                        int width = Math.Abs(tempPoints[args.Player.Index][0].X - tempPoints[args.Player.Index][1].X);
                        int height = Math.Abs(tempPoints[args.Player.Index][0].Y - tempPoints[args.Player.Index][1].Y);
                        int trueX = tempPoints[args.Player.Index][0].X;
                        int trueY = tempPoints[args.Player.Index][0].Y;
                        int trueX2 = tempPoints[args.Player.Index][1].X;
                        int trueY2 = tempPoints[args.Player.Index][1].Y;
                        /*if (x == trueX)
                        {

                            if (y == trueY)
                            {

                                for (int y2 = height; y2 >= 0; y2--)
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

                                for (int y2 = height; y2 >= 0; y2--)
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

                                for (int y2 = height; y2 >= 0; y2--)
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

                                for (int y2 = height; y2 >= 0; y2--)
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
                        for (int y2 = height; y2 >= 0; y2--)
                        {

                            for (int x2 = 0; x2 <= width; x2++)
                            {

                                updateTile(x + x2, y + y2);

                            }

                        }*/
                        if (!isWall)
                        {
                            drawLine(trueX, trueY, trueX2, trueY2, tileType, 255, brushStroke[args.Player.Index]);
                        }
                        else
                        {
                            drawLine(trueX, trueY, trueX2, trueY2, 255, wallType, brushStroke[args.Player.Index]);
                        }
                        lastAreas[args.Player.Index] = tempPoints[args.Player.Index];
                        clearTempPoints((byte)args.Player.Index);
                        
                        args.Player.SendMessage("Tiles changed!");
                    }
                    else
                    {

                        args.Player.SendMessage("That is not a recognized tile type.", Color.Red);

                    }
                }
                else
                {
                    args.Player.SendMessage("Points not set up yet", Color.Red);
                }
            }
            else
            {

                args.Player.SendMessage("Improper syntax! Proper syntax: /line blocktype", Color.Red);

            }

        }

        public static void Replace(CommandArgs args)
        {

            if (args.Parameters.Count > 1)
            {

                if (tempPoints[args.Player.Index].Count() > 1)
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
                            int x = Math.Min(tempPoints[args.Player.Index][0].X, tempPoints[args.Player.Index][1].X);
                            int y = Math.Min(tempPoints[args.Player.Index][0].Y, tempPoints[args.Player.Index][1].Y);
                            int width = Math.Abs(tempPoints[args.Player.Index][0].X - tempPoints[args.Player.Index][1].X);
                            int height = Math.Abs(tempPoints[args.Player.Index][0].Y - tempPoints[args.Player.Index][1].Y);
                            for (int y2 = height; y2 >= 0; y2--)
                            {

                                for (int x2 = 0; x2 <= width; x2++)
                                {

                                    if (!isWall)
                                    {
                                        if (((Main.tile[x + x2, y + y2].type == tileType) && (Main.tile[x + x2, y + y2].active)) || ((tileType == 253) && (Main.tile[x + x2, y + y2].lava == false) && (Main.tile[x + x2, y + y2].liquid > 0)) || ((tileType == 254) && (Main.tile[x + x2, y + y2].lava == true) && (Main.tile[x + x2, y + y2].liquid > 0)) || ((tileType == 252) && (!Main.tile[x + x2, y + y2].active)) || ((tileType == 251) && (Main.tile[x + x2, y + y2].wall == 0)) || ((tileType == 250) && (!Main.tile[x + x2, y + y2].active) && (Main.tile[x + x2, y + y2].wall == 0)))
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
                            for (int y2 = height; y2 >= 0; y2--)
                            {

                                for (int x2 = 0; x2 <= width; x2++)
                                {

                                    updateTile(x + x2, y + y2);

                                }

                            }
                            lastAreas[args.Player.Index] = tempPoints[args.Player.Index];
                            clearTempPoints((byte)args.Player.Index);
                            
                            args.Player.SendMessage("Tiles changed!");
                        }
                        else
                        {


                            args.Player.SendMessage(theString + " is not a recognized tile type.", Color.Red);

                        }
                    }
                    else
                    {

                        args.Player.SendMessage(args.Parameters[0] + " is not a recognized tile type.", Color.Red);

                    }
                }
                else
                {
                    args.Player.SendMessage("Points not set up yet", Color.Red);
                }

            }
            else
            {

                args.Player.SendMessage("Improper Syntax. Proper Syntax: /replace tiletype1 tiletype2", Color.Red);

            }

        }

        public static void Oval(CommandArgs args)
        {

            if (args.Parameters.Count > 0)
            {
                if (tempPoints[args.Player.Index].Count() > 1)
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
                        int x = Math.Min(tempPoints[args.Player.Index][0].X, tempPoints[args.Player.Index][1].X);
                        int y = Math.Min(tempPoints[args.Player.Index][0].Y, tempPoints[args.Player.Index][1].Y);
                        int width = Math.Abs(tempPoints[args.Player.Index][0].X - tempPoints[args.Player.Index][1].X);
                        int height = Math.Abs(tempPoints[args.Player.Index][0].Y - tempPoints[args.Player.Index][1].Y);
                        for (int y2 = height; y2 >= 0; y2--)
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
                        for (int y2 = height; y2 >= 0; y2--)
                        {

                            for (int x2 = 0; x2 <= width; x2++)
                            {

                                updateTile(x + x2, y + y2);

                            }

                        }
                        lastAreas[args.Player.Index] = tempPoints[args.Player.Index];
                        clearTempPoints((byte)args.Player.Index);
                        
                        args.Player.SendMessage("Tiles changed!");
                    }
                    else
                    {

                        args.Player.SendMessage("That is not a recognized tile type.", Color.Red);

                    }
                }
                else
                {
                    args.Player.SendMessage("Points not set up yet", Color.Red);
                }
            }
            else
            {

                args.Player.SendMessage("Improper syntax! Proper syntax: /oval blocktype", Color.Red);

            }

        }

        public static void SemiOval(CommandArgs args)
        {

            if (args.Parameters.Count > 1)
            {
                if (tempPoints[args.Player.Index].Count() > 1)
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
                        int x = Math.Min(tempPoints[args.Player.Index][0].X, tempPoints[args.Player.Index][1].X);
                        int y = Math.Min(tempPoints[args.Player.Index][0].Y, tempPoints[args.Player.Index][1].Y);
                        int width = Math.Abs(tempPoints[args.Player.Index][0].X - tempPoints[args.Player.Index][1].X);
                        int height = Math.Abs(tempPoints[args.Player.Index][0].Y - tempPoints[args.Player.Index][1].Y);
                        for (int y2 = height; y2 >= 0; y2--)
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
                                    default: args.Player.SendMessage("Improper syntax! Proper syntax: /semioval top|left|bottom|right blocktype", Color.Red); return;

                                }

                            }

                        }
                        for (int y2 = height; y2 >= 0; y2--)
                        {

                            for (int x2 = 0; x2 <= width; x2++)
                            {

                                updateTile(x + x2, y + y2);

                            }

                        }
                        lastAreas[args.Player.Index] = tempPoints[args.Player.Index];
                        clearTempPoints((byte)args.Player.Index);
                        
                        args.Player.SendMessage("Tiles changed!");
                    }
                    else
                    {

                        args.Player.SendMessage("That is not a recognized tile type.", Color.Red);

                    }
                }
                else
                {
                    args.Player.SendMessage("Points not set up yet", Color.Red);
                }
            }
            else
            {

                args.Player.SendMessage("Improper syntax! Proper syntax: /semioval top|left|bottom|right blocktype", Color.Red);

            }

        }

        public static void SemiOvalOutline(CommandArgs args)
        {

            if (args.Parameters.Count > 1)
            {
                if (tempPoints[args.Player.Index].Count() > 1)
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
                        int x = Math.Min(tempPoints[args.Player.Index][0].X, tempPoints[args.Player.Index][1].X);
                        int y = Math.Min(tempPoints[args.Player.Index][0].Y, tempPoints[args.Player.Index][1].Y);
                        int width = Math.Abs(tempPoints[args.Player.Index][0].X - tempPoints[args.Player.Index][1].X);
                        int height = Math.Abs(tempPoints[args.Player.Index][0].Y - tempPoints[args.Player.Index][1].Y);
                        for (int y2 = height; y2 >= 0; y2--)
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
                                    default: args.Player.SendMessage("Improper syntax! Proper syntax: /semiovaloutline top|left|bottom|right blocktype", Color.Red); return;

                                }

                            }

                        }
                        for (int y2 = height; y2 >= 0; y2--)
                        {

                            for (int x2 = 0; x2 <= width; x2++)
                            {

                                updateTile(x + x2, y + y2);

                            }

                        }
                        lastAreas[args.Player.Index] = tempPoints[args.Player.Index];
                        clearTempPoints((byte)args.Player.Index);
                        
                        args.Player.SendMessage("Tiles changed!");
                    }
                    else
                    {

                        args.Player.SendMessage("That is not a recognized tile type.", Color.Red);

                    }
                }
                else
                {
                    args.Player.SendMessage("Points not set up yet", Color.Red);
                }
            }
            else
            {

                args.Player.SendMessage("Improper syntax! Proper syntax: /semiovaloutline top|left|bottom|right blocktype", Color.Red);

            }

        }

        public static void OvalOutline(CommandArgs args)
        {

            if (args.Parameters.Count > 0)
            {
                if (tempPoints[args.Player.Index].Count() > 1)
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
                        int x = Math.Min(tempPoints[args.Player.Index][0].X, tempPoints[args.Player.Index][1].X);
                        int y = Math.Min(tempPoints[args.Player.Index][0].Y, tempPoints[args.Player.Index][1].Y);
                        int width = Math.Abs(tempPoints[args.Player.Index][0].X - tempPoints[args.Player.Index][1].X);
                        int height = Math.Abs(tempPoints[args.Player.Index][0].Y - tempPoints[args.Player.Index][1].Y);
                        for (int y2 = height; y2 >= 0; y2--)
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
                        for (int y2 = height; y2 >= 0; y2--)
                        {

                            for (int x2 = 0; x2 <= width; x2++)
                            {

                                updateTile(x + x2, y + y2);

                            }

                        }
                        lastAreas[args.Player.Index] = tempPoints[args.Player.Index];
                        clearTempPoints((byte)args.Player.Index);
                        
                        args.Player.SendMessage("Tiles changed!");
                    }
                    else
                    {

                        args.Player.SendMessage("That is not a recognized tile type.", Color.Red);

                    }
                }
                else
                {
                    args.Player.SendMessage("Points not set up yet", Color.Red);
                }
            }
            else
            {

                args.Player.SendMessage("Improper syntax! Proper syntax: /ovaloutline blocktype", Color.Red);

            }

        }

        public static void Paste(CommandArgs args)
        {

            if ((copyW[args.Player.Index] != 0) || (copyH[args.Player.Index] != 0))
            {

                if (tempPoints[args.Player.Index].Count() > 0)
                {

                    int X, Y;
                    int x = tempPoints[args.Player.Index][0].X;
                    int y = tempPoints[args.Player.Index][0].Y;
                    int width = copyW[args.Player.Index];
                    int height = copyH[args.Player.Index];
                    int i = args.Player.Index;
                    if (rectCollision(copyX[i], copyY[i], copyX[i] + width, copyY[i] + height, x, y, x + width, y + height))
                    {

                        args.Player.SendMessage("You cannot paste into the cut/copied area.", Color.Red);
                        return;

                    }
                    for (int y2 = height; y2 >= 0; y2--)
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
                    for (int y2 = height; y2 >= 0; y2--)
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
                    for (int y2 = height; y2 >= 0; y2--)
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
                    for (int y2 = height; y2 >= 0; y2--)
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
                        for (int y2 = height; y2 >= 0; y2--)
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
                        for (int y2 = height; y2 >= 0; y2--)
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
                    lastAreas[args.Player.Index] = tempPoints[args.Player.Index];
                    clearTempPoints((byte)args.Player.Index);
                    
                    args.Player.SendMessage("You have pasted from the clipboard.");
                }
                else
                {
                    args.Player.SendMessage("Point not set up yet", Color.Red);
                }
            }
            else
            {

                args.Player.SendMessage("You need to copy something first!", Color.Red);

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
                    default: args.Player.SendMessage("Improper syntax.  Proper Syntax: /copy [all|front|back]", Color.Red); return;

                }

            }
            else
            {

                clipboardType[args.Player.Index] = 0;

            }
            cut[args.Player.Index] = false;
            if (tempPoints[args.Player.Index].Count() > 1)
            {

                copyX[args.Player.Index] = Math.Min(tempPoints[args.Player.Index][0].X, tempPoints[args.Player.Index][1].X);
                copyY[args.Player.Index] = Math.Min(tempPoints[args.Player.Index][0].Y, tempPoints[args.Player.Index][1].Y);
                copyW[args.Player.Index] = Math.Abs(tempPoints[args.Player.Index][0].X - tempPoints[args.Player.Index][1].X);
                copyH[args.Player.Index] = Math.Abs(tempPoints[args.Player.Index][0].Y - tempPoints[args.Player.Index][1].Y);
                lastAreas[args.Player.Index] = tempPoints[args.Player.Index];
                clearTempPoints((byte)args.Player.Index);
                
                args.Player.SendMessage("You have successfully copied to the clipboard.");
            }
            else
            {
                args.Player.SendMessage("Points not set up yet", Color.Red);
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
                    default: args.Player.SendMessage("Improper syntax.  Proper Syntax: /cut [all|front|back]", Color.Red); return;

                }

            }
            else
            {

                clipboardType[args.Player.Index] = 0;

            }
            cut[args.Player.Index] = true;
            if (tempPoints[args.Player.Index].Count() > 1)
            {

                copyX[args.Player.Index] = Math.Min(tempPoints[args.Player.Index][0].X, tempPoints[args.Player.Index][1].X);
                copyY[args.Player.Index] = Math.Min(tempPoints[args.Player.Index][0].Y, tempPoints[args.Player.Index][1].Y);
                copyW[args.Player.Index] = Math.Abs(tempPoints[args.Player.Index][0].X - tempPoints[args.Player.Index][1].X);
                copyH[args.Player.Index] = Math.Abs(tempPoints[args.Player.Index][0].Y - tempPoints[args.Player.Index][1].Y);
                lastAreas[args.Player.Index] = tempPoints[args.Player.Index];
                clearTempPoints((byte)args.Player.Index);
                
                args.Player.SendMessage("You have successfully cut to the clipboard.");
            }
            else
            {
                args.Player.SendMessage("Points not set up yet", Color.Red);
            }

        }

        public static void ClearArea(CommandArgs args)
        {

            if (args.Parameters.Count >= 1)
            {
                if (tempPoints[args.Player.Index].Count() > 1)
                {

                    int x = Math.Min(tempPoints[args.Player.Index][0].X, tempPoints[args.Player.Index][1].X);
                    int y = Math.Min(tempPoints[args.Player.Index][0].Y, tempPoints[args.Player.Index][1].Y);
                    int width = Math.Abs(tempPoints[args.Player.Index][0].X - tempPoints[args.Player.Index][1].X);
                    int height = Math.Abs(tempPoints[args.Player.Index][0].Y - tempPoints[args.Player.Index][1].Y);
                    switch (args.Parameters[0].ToLower())
                    {
                        case "all": for (int y2 = height; y2 >= 0; y2--)
                            {

                                for (int x2 = 0; x2 <= width; x2++)
                                {

                                    Main.tile[x + x2, y + y2].active = false;
                                    Main.tile[x + x2, y + y2].wall = 0;
                                    Main.tile[x + x2, y + y2].skipLiquid = true;
                                    Main.tile[x + x2, y + y2].liquid = 0;

                                }

                            } break;
                        case "back": for (int y2 = height; y2 >= 0; y2--)
                            {

                                for (int x2 = 0; x2 <= width; x2++)
                                {

                                    Main.tile[x + x2, y + y2].wall = 0;

                                }

                            } break;
                        case "front": for (int y2 = height; y2 >= 0; y2--)
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
                    for (int y2 = height; y2 >= 0; y2--)
                    {

                        for (int x2 = 0; x2 <= width; x2++)
                        {

                            updateTile(x + x2, y + y2);

                        }

                    }
                    lastAreas[args.Player.Index] = tempPoints[args.Player.Index];
                    clearTempPoints((byte)args.Player.Index);
                    
                    args.Player.SendMessage("Tiles cleared!");
                }
                else
                {
                    args.Player.SendMessage("Points not set up yet", Color.Red);
                }
            }
            else
            {

                if (tempPoints[args.Player.Index].Count() > 1)
                {

                    int x = Math.Min(tempPoints[args.Player.Index][0].X, tempPoints[args.Player.Index][1].X);
                    int y = Math.Min(tempPoints[args.Player.Index][0].Y, tempPoints[args.Player.Index][1].Y);
                    int width = Math.Abs(tempPoints[args.Player.Index][0].X - tempPoints[args.Player.Index][1].X);
                    int height = Math.Abs(tempPoints[args.Player.Index][0].Y - tempPoints[args.Player.Index][1].Y);
                    for (int y2 = height; y2 >= 0; y2--)
                    {

                        for (int x2 = 0; x2 <= width; x2++)
                        {

                            Main.tile[x + x2, y + y2].active = false;
                            Main.tile[x + x2, y + y2].wall = 0;
                            Main.tile[x + x2, y + y2].skipLiquid = true;
                            Main.tile[x + x2, y + y2].liquid = 0;

                        }

                    }
                    for (int y2 = height; y2 >= 0; y2--)
                    {

                        for (int x2 = 0; x2 <= width; x2++)
                        {

                            updateTile(x + x2, y + y2);

                        }

                    }
                    lastAreas[args.Player.Index] = tempPoints[args.Player.Index];
                    clearTempPoints((byte)args.Player.Index);
                    
                    args.Player.SendMessage("Tiles cleared!");
                }
                else
                {
                    args.Player.SendMessage("Points not set up yet", Color.Red);
                }

            }
        }

        public static void Rectangle(CommandArgs args)
        {
            
            if (args.Parameters.Count > 0)
            {
                if (tempPoints[args.Player.Index].Count() > 1)
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
                        int x = Math.Min(tempPoints[args.Player.Index][0].X, tempPoints[args.Player.Index][1].X);
                        int y = Math.Min(tempPoints[args.Player.Index][0].Y, tempPoints[args.Player.Index][1].Y);
                        int width = Math.Abs(tempPoints[args.Player.Index][0].X - tempPoints[args.Player.Index][1].X);
                        int height = Math.Abs(tempPoints[args.Player.Index][0].Y - tempPoints[args.Player.Index][1].Y);
                        for (int y2 = height; y2 >= 0; y2--)
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
                        for (int y2 = height; y2 >= 0; y2--)
                        {

                            for (int x2 = 0; x2 <= width; x2++)
                            {

                                updateTile(x + x2, y + y2);

                            }

                        }
                        lastAreas[args.Player.Index] = tempPoints[args.Player.Index];
                        clearTempPoints((byte)args.Player.Index);
                        
                        args.Player.SendMessage("Tiles changed!"); return;
                    }
                    else
                    {

                        args.Player.SendMessage("That is not a recognized tile type.", Color.Red);

                    }
                }
                else
                {
                    args.Player.SendMessage("Points not set up yet", Color.Red);
                }
            }
            else
            {

                args.Player.SendMessage("Improper syntax! Proper syntax: /rectangle blocktype", Color.Red);

            }
        }

        public static void RectangleOutline(CommandArgs args)
        {

            if (args.Parameters.Count > 0)
            {
                if (tempPoints[args.Player.Index].Count() > 1)
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
                        int x = Math.Min(tempPoints[args.Player.Index][0].X, tempPoints[args.Player.Index][1].X);
                        int y = Math.Min(tempPoints[args.Player.Index][0].Y, tempPoints[args.Player.Index][1].Y);
                        int width = Math.Abs(tempPoints[args.Player.Index][0].X - tempPoints[args.Player.Index][1].X);
                        int height = Math.Abs(tempPoints[args.Player.Index][0].Y - tempPoints[args.Player.Index][1].Y);
                        for (int y2 = height + (int)(brushStroke[args.Player.Index] / 2); y2 >= -(int)(brushStroke[args.Player.Index] / 2); y2--)
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
                        lastAreas[args.Player.Index] = tempPoints[args.Player.Index];
                        clearTempPoints((byte)args.Player.Index);
                        
                        args.Player.SendMessage("Tiles changed!");
                    }
                    else
                    {

                        args.Player.SendMessage("That is not a recognized tile type.", Color.Red);

                    }
                }
                else
                {
                    args.Player.SendMessage("Points not set up yet", Color.Red);
                }
            }
            else
            {

                args.Player.SendMessage("Improper syntax! Proper syntax: /rectangleoutline blocktype", Color.Red);

            }
        }

        public static void Circle(CommandArgs args)
        {

            if (args.Parameters.Count > 1)
            {
                if (tempPoints[args.Player.Index].Count() > 0)
                {
                    int radius;
                    try { radius = Convert.ToInt32(args.Parameters[0]); }
                    catch (Exception) { args.Player.SendMessage("The radius needs to be an integer.", Color.Red); return; }
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
                        int x = tempPoints[args.Player.Index][0].X;
                        int y = tempPoints[args.Player.Index][0].Y;
                        for (int y2 = radius; y2 >= -radius; y2--)
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
                        lastAreas[args.Player.Index] = tempPoints[args.Player.Index];
                        clearTempPoints((byte)args.Player.Index);
                        
                        args.Player.SendMessage("Tiles changed!");
                    }
                    else
                    {

                        args.Player.SendMessage("That is not a recognized tile type.", Color.Red);

                    }
                }
                else
                {
                    args.Player.SendMessage("Point 1 not set up yet", Color.Red);
                }
            }
            else
            {

                args.Player.SendMessage("Improper syntax! Proper syntax: /circle radius blocktype", Color.Red);

            }

        }

        public static void CircleOutline(CommandArgs args)
        {

            if (args.Parameters.Count > 1)
            {
                if (tempPoints[args.Player.Index].Count() > 0)
                {
                    int radius;
                    try { radius = Convert.ToInt32(args.Parameters[0]); }
                    catch (Exception) { args.Player.SendMessage("The radius needs to be an integer.", Color.Red); return; }
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
                        int x = tempPoints[args.Player.Index][0].X;
                        int y = tempPoints[args.Player.Index][0].Y;
                        for (int y2 = radius + (int)(brushStroke[args.Player.Index] / 2); y2 >= -radius - (int)(brushStroke[args.Player.Index] / 2); y2--)
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
                        lastAreas[args.Player.Index] = tempPoints[args.Player.Index];
                        clearTempPoints((byte)args.Player.Index);
                        
                        args.Player.SendMessage("Tiles changed!");
                    }
                    else
                    {

                        args.Player.SendMessage("That is not a recognized tile type.", Color.Red);

                    }
                }
                else
                {
                    args.Player.SendMessage("Point 1 not set up yet", Color.Red);
                }
            }
            else
            {

                args.Player.SendMessage("Improper syntax! Proper syntax: /circleoutline radius blocktype", Color.Red);

            }

        }

        public static void SemiCircle(CommandArgs args)
        {

            if (args.Parameters.Count > 2)
            {
                if (tempPoints[args.Player.Index].Count() > 0)
                {
                    int radius;
                    try { radius = Convert.ToInt32(args.Parameters[1]); }
                    catch (Exception) { args.Player.SendMessage("The radius needs to be an integer.", Color.Red); return; }
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
                        int x = tempPoints[args.Player.Index][0].X;
                        int y = tempPoints[args.Player.Index][0].Y;
                        switch (args.Parameters[0].ToLower())
                        {

                            case "left": for (int y2 = radius; y2 >= -radius; y2--)
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
                            case "right": for (int y2 = radius; y2 >= -radius; y2--)
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
                            case "top": for (int y2 = 0; y2 >= -radius; y2--)
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
                            case "bottom": for (int y2 = radius; y2 >= 0; y2--)
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
                            default: args.Player.SendMessage("Improper syntax! Proper syntax: /semicircle top|left|bottom|right radius blocktype", Color.Red); return;

                        }
                        for (int y2 = -radius; y2 <= radius; y2++)
                        {

                            for (int x2 = -radius; x2 <= radius; x2++)
                            {

                                updateTile(x + x2, y + y2);

                            }

                        }
                        lastAreas[args.Player.Index] = tempPoints[args.Player.Index];
                        clearTempPoints((byte)args.Player.Index);
                        
                        args.Player.SendMessage("Tiles changed!");
                    }
                    else
                    {

                        args.Player.SendMessage("That is not a recognized tile type.", Color.Red);

                    }
                }
                else
                {
                    args.Player.SendMessage("Point 1 not set up yet", Color.Red);
                }
            }
            else
            {

                args.Player.SendMessage("Improper syntax! Proper syntax: /semicircle top|left|bottom|right radius blocktype", Color.Red);

            }

        }

        public static void SemiCircleOutline(CommandArgs args)
        {

            if (args.Parameters.Count > 2)
            {
                if (tempPoints[args.Player.Index].Count() > 0)
                {
                    int radius;
                    try { radius = Convert.ToInt32(args.Parameters[1]); }
                    catch (Exception) { args.Player.SendMessage("The radius needs to be an integer.", Color.Red); return; }
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
                        int x = tempPoints[args.Player.Index][0].X;
                        int y = tempPoints[args.Player.Index][0].Y;
                        switch (args.Parameters[0].ToLower())
                        {

                            case "left": for (int y2 = radius + (int)(brushStroke[args.Player.Index] / 2); y2 >= -radius - (int)(brushStroke[args.Player.Index] / 2); y2--)
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
                            case "right": for (int y2 = radius + (int)(brushStroke[args.Player.Index] / 2); y2 >= -radius - (int)(brushStroke[args.Player.Index] / 2); y2--)
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
                            case "top": for (int y2 = 0; y2 >= -radius - (int)(brushStroke[args.Player.Index] / 2); y2--)
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
                            case "bottom": for (int y2 = radius + (int)(brushStroke[args.Player.Index] / 2); y2 >= 0; y2--)
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
                            default: args.Player.SendMessage("Improper syntax! Proper syntax: /semicircleoutline top|left|bottom|right radius blocktype", Color.Red); return;

                        }
                        for (int y2 = -radius - (int)(brushStroke[args.Player.Index] / 2); y2 <= radius + (int)(brushStroke[args.Player.Index] / 2); y2++)
                        {

                            for (int x2 = -radius - (int)(brushStroke[args.Player.Index] / 2); x2 <= radius + (int)(brushStroke[args.Player.Index] / 2); x2++)
                            {

                                updateTile(x + x2, y + y2);

                            }

                        }
                        lastAreas[args.Player.Index] = tempPoints[args.Player.Index];
                        clearTempPoints((byte)args.Player.Index);
                        
                        args.Player.SendMessage("Tiles changed!");
                    }
                    else
                    {

                        args.Player.SendMessage("That is not a recognized tile type.", Color.Red);

                    }
                }
                else
                {
                    args.Player.SendMessage("Point 1 not set up yet", Color.Red);
                }
            }
            else
            {

                args.Player.SendMessage("Improper syntax! Proper syntax: /semicircleoutline top|left|bottom|right radius blocktype", Color.Red);

            }

        }

        public static void changeTile(int x, int y, byte type, byte wall)
        {

            if (type < 250)
            {

                Main.tile[x, y].type = type;
                Main.tile[x, y].active = true;
                Main.tile[x, y].liquid = 0;
                Main.tile[x, y].skipLiquid = true;
                Main.tile[x, y].frameNumber = 0;
                Main.tile[x, y].frameX = -1;
                Main.tile[x, y].frameY = -1;

            }
            else if (type == 250)
            {

                Main.tile[x, y].active = false;
                Main.tile[x, y].wall = 0;
                Main.tile[x, y].skipLiquid = true;
                Main.tile[x, y].liquid = 0;

            }
            else if (type == 251)
            {

                Main.tile[x, y].wall = 0;

            }
            else if (type == 252)
            {

                Main.tile[x, y].active = false;
                Main.tile[x, y].skipLiquid = true;
                Main.tile[x, y].liquid = 0;

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
            if ((Main.tile[x, y].type == 53) || (Main.tile[x, y].type == 253) || (Main.tile[x, y].type == 254) || (Main.tile[x,y].type == 112))
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
            foreach (Terraria.ServerSock theSock in Netplay.serverSock)
            {
                theSock.tileSection[x, y] = false;
            }

        }

        public static void drawLine(int trueX, int trueY, int trueX2, int trueY2, byte tiletype, byte walltype, double lineBrushStroke)
        {
            /*int height = y2 - y1;
            int width = x2 - x1;
            double angle = Math.Atan2(height, width) + Math.PI / 2;
            for (double step = -lineBrushStroke / 2; step <= lineBrushStroke / 2; step += .1)
            {

                int x3 = (int)(x1 + Math.Cos(angle) * step);
                int y3 = (int)(y1 + Math.Sin(angle) * step);
                int x4 = (int)(x2 + Math.Cos(angle) * step);
                int y4 = (int)(y2 + Math.Sin(angle) * step);
                drawSimpleLine(x3, y3, x4, y4, tiletype, walltype);

            }*/
            int x = Math.Min(trueX, trueX2);
            int y = Math.Min(trueY, trueY2);
            int width = Math.Abs(trueX - trueX2);
            int height = Math.Abs(trueY - trueY2);
            if (x == trueX)
            {

                if (y == trueY)
                {

                    for (int y2 = height; y2 >= 0; y2--)
                    {

                        for (int x2 = 0; x2 <= width; x2++)
                        {

                            if (Math.Abs(-(height / (double)width) * (x2) + y2) / Math.Sqrt(Math.Pow(height / (double)width, 2) + 1) <= lineBrushStroke / 2)
                            {

                                    changeTile(x + x2, y + y2, tiletype, walltype);

                            }

                        }

                    }

                }
                else
                {

                    for (int y2 = height; y2 >= 0; y2--)
                    {

                        for (int x2 = 0; x2 <= width; x2++)
                        {

                            if (Math.Abs(-(-height / (double)width) * (x2) + y2 - height) / Math.Sqrt(Math.Pow(-height / (double)width, 2) + 1) <= lineBrushStroke / 2)
                            {

                                    changeTile(x + x2, y + y2, tiletype, walltype);

                            }

                        }

                    }

                }

            }
            else
            {

                if (y == trueY)
                {

                    for (int y2 = height; y2 >= 0; y2--)
                    {

                        for (int x2 = 0; x2 <= width; x2++)
                        {

                            if (Math.Abs(-(height / -(double)width) * (x2 - width) + y2) / Math.Sqrt(Math.Pow(height / -(double)width, 2) + 1) <= lineBrushStroke / 2)
                            {

                                    changeTile(x + x2, y + y2, tiletype, walltype);

                            }

                        }

                    }

                }
                else
                {

                    for (int y2 = height; y2 >= 0; y2--)
                    {

                        for (int x2 = 0; x2 <= width; x2++)
                        {

                            if (Math.Abs(-(-height / -(double)width) * (x2 - width) + y2 - height) / Math.Sqrt(Math.Pow(-height / -(double)width, 2) + 1) <= lineBrushStroke / 2)
                            {

                                    changeTile(x + x2, y + y2, tiletype, walltype);
                                
                            }

                        }

                    }

                }

            }
            for (int y2 = height; y2 >= 0; y2--)
            {

                for (int x2 = 0; x2 <= width; x2++)
                {

                    updateTile(x + x2, y + y2);

                }

            }

        }
        public static void drawSimpleLine(int x1, int y1, int x2, int y2, byte tiletype, byte walltype)
        {
            int height = y2 - y1;
            int width = x2 - x1;
            double slope = ((double)height) / ((double)width);
            int length1D = Math.Max(Math.Abs(height), Math.Abs(width));
            for (double step = 0; step <= length1D; step += .5)
            {

                int x3 = x1 + (int)(((double)width / length1D) * step);
                int y3 = y1 + (int)(((double)height / length1D) * step);
                changeTile(x3, y3, tiletype, walltype);

            }
            for (double step = 0; step <= length1D; step += .5)
            {

                int x3 = (int)(((double)width / length1D) * step);
                int y3 = (int)(((double)height / length1D) * step);
                updateTile(x1 + x3, y1 + y3);

            }

        }
        public static List<Point> getTempOutline(List<Point> theTempPoint)
        {

            Point[] thePoint = new Point[theTempPoint.Count()];
            theTempPoint.CopyTo(thePoint);
            List<Point> tempPoint = new List<Point>();
            tempPoint.AddRange(thePoint);
            if (tempPoint.Count() == 2) {

                int theX1 = tempPoint[0].X;
                int theX2 = tempPoint[1].X;
                int theY1 = tempPoint[0].Y;
                int theY2 = tempPoint[1].Y;
                for (int x = 0; x <= Math.Abs(theX1 - theX2); x++)
                {

                    tempPoint.Add(new Point(x + Math.Min(theX1, theX2), theY1));
                    tempPoint.Add(new Point(x + Math.Min(theX1, theX2), theY2));

                }
                for (int y = 0; y <= Math.Abs(theY1 - theY2); y++)
                {

                    tempPoint.Add(new Point(theX1, y + Math.Min(theY1, theY2)));
                    tempPoint.Add(new Point(theX2, y + Math.Min(theY1, theY2)));

                }

            } else if (tempPoint.Count() > 2) {

                int theLength = tempPoint.Count();
                for (int i = 0; i < theLength - 1; i++)
                {

                    int theX1 = tempPoint[i].X;
                    int theX2 = tempPoint[i + 1].X;
                    int theY1 = tempPoint[i].Y;
                    int theY2 = tempPoint[i + 1].Y;
                    int height = theY2 - theY1;
                    int width = theX2 - theX1;
                    int length1D = Math.Max(Math.Abs(height), Math.Abs(width));
                    for (double step = 0; step <= length1D; step += .5)
                    {

                        if (length1D != 0)
                        {

                            int x3 = theX1 + (int)(((double)width / length1D) * step);
                            int y3 = theY1 + (int)(((double)height / length1D) * step);
                            tempPoint.Add(new Point(x3, y3));

                        }
                        else
                        {

                            int x3 = theX1;
                            int y3 = theY1;
                            tempPoint.Add(new Point(x3, y3));

                        }

                    }

                }

            }
            return (tempPoint);

        }
        public static void clearTempPoints(byte ply)
        {

            List<Point> evenMoreTempPoints = getTempOutline(tempPoints[ply]);
            foreach (Point thePoint in evenMoreTempPoints)
            {

                NetMessage.SendTileSquare(ply, thePoint.X, thePoint.Y, 1);

            }
            tempPoints[ply] = new List<Point>();
            awaitingPoint[ply] = 0;

        }
        public static void refreshTempTiles(int ply, List<Point> tempTempPoint)
        {

            if (tempPoints[ply].Count() >= 1)
            {

                List<Point> tempPointList = getTempOutline(tempPoints[ply]);
                foreach (Point thePoint in tempPointList)
                {
                    byte tempType = Main.tile[thePoint.X, thePoint.Y].type;
                    bool tempActive = Main.tile[thePoint.X, thePoint.Y].active;
                    Main.tile[thePoint.X, thePoint.Y].type = 70;
                    Main.tile[thePoint.X, thePoint.Y].active = true;
                    NetMessage.SendTileSquare(ply, thePoint.X, thePoint.Y, 1);
                    Main.tile[thePoint.X, thePoint.Y].type = tempType;
                    Main.tile[thePoint.X, thePoint.Y].active = tempActive;
                    while (tempTempPoint.Contains(new Point(thePoint.X, thePoint.Y)))
                    {

                        tempTempPoint.Remove(new Point(thePoint.X, thePoint.Y));

                    }
                }
                foreach (Point thePoint in tempTempPoint)
                {

                    NetMessage.SendTileSquare(ply, thePoint.X, thePoint.Y, 1);

                }

            }

        }
    }
}