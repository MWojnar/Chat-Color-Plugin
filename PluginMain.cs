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
using System.Linq;
using System.Threading;

namespace PluginTemplate
{
    [APIVersion(1, 8)]
    public class PluginTemplate : TerrariaPlugin
    {
        public static SqlTableEditor SQLEditor;
        public static SqlTableCreator SQLWriter;
        public override string Name
        {
            get { return "Chat Colors"; }
        }
        public override string Author
        {
            get { return "Created by DaGamesta"; }
        }
        public override string Description
        {
            get { return "Ooh, the colors! :D"; }
        }
        public override Version Version
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version; }
        }

        public override void Initialize()
        {
            GameHooks.Initialize += OnInitialize;
            ServerHooks.Chat += OnChat;
        }
        public override void DeInitialize()
        {
            GameHooks.Initialize -= OnInitialize;
            ServerHooks.Chat -= OnChat;
        }
        public PluginTemplate(Main game)
            : base(game)
        {
            Order = -2;
        }

        public void OnInitialize()
        {
            SQLEditor = new SqlTableEditor(TShock.DB, TShock.DB.GetSqlType() == SqlType.Sqlite ? (IQueryBuilder)new SqliteQueryCreator() : new MysqlQueryCreator());
            SQLWriter = new SqlTableCreator(TShock.DB, TShock.DB.GetSqlType() == SqlType.Sqlite ? (IQueryBuilder)new SqliteQueryCreator() : new MysqlQueryCreator());
            var table = new SqlTable("ChatColor",
                        new SqlColumn("Name", MySqlDbType.Text) { Unique = true },
                        new SqlColumn("CanChange", MySqlDbType.Int32),
                        new SqlColumn("R", MySqlDbType.Int32),
                        new SqlColumn("G", MySqlDbType.Int32),
                        new SqlColumn("B", MySqlDbType.Int32));
            SQLWriter.EnsureExists(table);
            bool color = false;
            bool othercolor = false;

            foreach (Group group in TShock.Groups.groups)
            {
                if (group.Name != "superadmin")
                {
                    if (group.HasPermission("chatcolor"))
                        color = true;
                    if (group.HasPermission("otherchatcolor"))
                        othercolor = true;
                }
            }
            List<string> permlist = new List<string>();
            if (!color)
                permlist.Add("chatcolor");
            TShock.Groups.AddPermissions("trustedadmin", permlist);
            permlist = new List<string>();
            if (!othercolor)
                permlist.Add("otherchatcolor");
            TShock.Groups.AddPermissions("trustedadmin", permlist);

            Commands.ChatCommands.Add(new Command("chatcolor", ChatColor, "chatcolor"));
            Commands.ChatCommands.Add(new Command("otherchatcolor", OtherChatColor, "otherchatcolor"));
        }

        

        public void OnChat(messageBuffer msg, int ply, string text, HandledEventArgs e)
        {
            if (TShock.Players[ply].IsLoggedIn)
            {

                int RowID = SearchTable(SQLEditor.ReadColumn("ChatColor", "Name", new List<SqlValue>()), TShock.Players[ply].UserAccountName);
                if (RowID != -1)
                {

                    TShock.Players[ply].Group.R = Convert.ToByte(SQLEditor.ReadColumn("ChatColor", "R", new List<SqlValue>())[RowID].ToString());
                    TShock.Players[ply].Group.G = Convert.ToByte(SQLEditor.ReadColumn("ChatColor", "G", new List<SqlValue>())[RowID].ToString());
                    TShock.Players[ply].Group.B = Convert.ToByte(SQLEditor.ReadColumn("ChatColor", "B", new List<SqlValue>())[RowID].ToString());

                }
                else
                {

                    TShock.Players[ply].Group.R = 255;
                    TShock.Players[ply].Group.G = 255;
                    TShock.Players[ply].Group.B = 255;

                }

            }
            else
            {

                TShock.Players[ply].Group.R = 255;
                TShock.Players[ply].Group.G = 255;
                TShock.Players[ply].Group.B = 255;

            }
        }

        public static void ChatColor(CommandArgs args)
        {
            if (args.Parameters.Count >= 3)
            {
                string[] text;
                text = new string[100];
                int i = 0;

                foreach (string word in args.Parameters)
                {
                    text[i] = word;
                    i += 1;
                }
                byte text1, text2, text3;

                try { text1 = Convert.ToByte(text[0]); }
                catch (System.FormatException) { args.Player.SendMessage("The Red value was not a proper integer.", System.Drawing.Color.Red); return; }
                catch (System.OverflowException) { args.Player.SendMessage("The Red value was not an integer between 0 and 255.", System.Drawing.Color.Red); return; }
                try { text2 = Convert.ToByte(text[1]); }
                catch (System.FormatException) { args.Player.SendMessage("The Green value was not a proper integer.", System.Drawing.Color.Red); return; }
                catch (System.OverflowException) { args.Player.SendMessage("The Green value was not an integer between 0 and 255.", System.Drawing.Color.Red); return; }
                try { text3 = Convert.ToByte(text[2]); }
                catch (System.FormatException) { args.Player.SendMessage("The Blue value was not a proper integer.", System.Drawing.Color.Red); return; }
                catch (System.OverflowException) { args.Player.SendMessage("The Blue value was not an integer between 0 and 255.", System.Drawing.Color.Red); return; }
                if (args.Player.IsLoggedIn)
                {
                    if (SearchTable(SQLEditor.ReadColumn("ChatColor", "Name", new List<SqlValue>()), args.Player.UserAccountName) != -1)
                    {

                        List<SqlValue> values = new List<SqlValue>();
                        values.Add(new SqlValue("R", text1));
                        values.Add(new SqlValue("G", text2));
                        values.Add(new SqlValue("B", text3));
                        List<SqlValue> where = new List<SqlValue>();
                        where.Add(new SqlValue("Name", "'" + args.Player.UserAccountName + "'"));
                        SQLEditor.UpdateValues("ChatColor", values, where);

                    }
                    else
                    {

                        List<SqlValue> list = new List<SqlValue>();
                        list.Add(new SqlValue("Name", "'" + args.Player.UserAccountName + "'"));
                        list.Add(new SqlValue("CanChange", 1));
                        list.Add(new SqlValue("R", text1));
                        list.Add(new SqlValue("G", text2));
                        list.Add(new SqlValue("B", text3));
                        SQLEditor.InsertValues("ChatColor", list);

                    }

                }
                else
                {

                    args.Player.SendMessage("You need to be logged in to use this command.", System.Drawing.Color.Red); return;

                }
                args.Player.SendMessage("This is your new text color!  You like? :)", text1, text2, text3); return;
            }
            else
            {

                args.Player.SendMessage("Not enough arguments. Format is /chatcolor Red Green Blue", System.Drawing.Color.Red);
                return;

            }
        }
        public static void OtherChatColor(CommandArgs args)
        {
            if (args.Parameters.Count >= 4)
            {
                string[] text;
                text = new string[100];
                int i = 0;

                foreach (string word in args.Parameters)
                {
                    text[i] = word;
                    i += 1;
                }
                byte text1, text2, text3;

                try { text1 = Convert.ToByte(text[0]); }
                catch (System.FormatException) { args.Player.SendMessage("The Red value was not a proper integer.", System.Drawing.Color.Red); return; }
                catch (System.OverflowException) { args.Player.SendMessage("The Red value was not an integer between 0 and 255.", System.Drawing.Color.Red); return; }
                try { text2 = Convert.ToByte(text[1]); }
                catch (System.FormatException) { args.Player.SendMessage("The Green value was not a proper integer.", System.Drawing.Color.Red); return; }
                catch (System.OverflowException) { args.Player.SendMessage("The Green value was not an integer between 0 and 255.", System.Drawing.Color.Red); return; }
                try { text3 = Convert.ToByte(text[2]); }
                catch (System.FormatException) { args.Player.SendMessage("The Blue value was not a proper integer.", System.Drawing.Color.Red); return; }
                catch (System.OverflowException) { args.Player.SendMessage("The Blue value was not an integer between 0 and 255.", System.Drawing.Color.Red); return; }
                    if (SearchTable(SQLEditor.ReadColumn("ChatColor", "Name", new List<SqlValue>()), text[3]) != -1)
                    {

                        List<SqlValue> values = new List<SqlValue>();
                        values.Add(new SqlValue("R", text1));
                        values.Add(new SqlValue("G", text2));
                        values.Add(new SqlValue("B", text3));
                        List<SqlValue> where = new List<SqlValue>();
                        where.Add(new SqlValue("Name", "'" + text[3] + "'"));
                        SQLEditor.UpdateValues("ChatColor", values, where);

                    }
                    else if (SearchTable(SQLEditor.ReadColumn("Users", "Username", new List<SqlValue>()), text[3]) != -1)
                    {

                        List<SqlValue> list = new List<SqlValue>();
                        list.Add(new SqlValue("Name", "'" + text[3] + "'"));
                        list.Add(new SqlValue("CanChange", 1));
                        list.Add(new SqlValue("R", text1));
                        list.Add(new SqlValue("G", text2));
                        list.Add(new SqlValue("B", text3));
                        SQLEditor.InsertValues("ChatColor", list);

                    }
                    else
                    {

                        var players = Tools.FindPlayer(text[3]);
                        if (players.Count == 0)
                        {

                            args.Player.SendMessage("Invalid player.", System.Drawing.Color.Red); return;

                        }
                        else if (players.Count > 1)
                        {

                            args.Player.SendMessage("More than one player matched.", System.Drawing.Color.Red); return;

                        }
                        else
                        {
                            if (players[0].IsLoggedIn)
                            {

                                if (SearchTable(SQLEditor.ReadColumn("ChatColor", "Name", new List<SqlValue>()), players[0].UserAccountName) != -1)
                                {

                                    List<SqlValue> values = new List<SqlValue>();
                                    values.Add(new SqlValue("R", text1));
                                    values.Add(new SqlValue("G", text2));
                                    values.Add(new SqlValue("B", text3));
                                    List<SqlValue> where = new List<SqlValue>();
                                    where.Add(new SqlValue("Name", "'" + players[0].UserAccountName + "'"));
                                    SQLEditor.UpdateValues("ChatColor", values, where);

                                }
                                else if (SearchTable(SQLEditor.ReadColumn("Users", "Username", new List<SqlValue>()), players[0].UserAccountName) != -1)
                                {

                                    List<SqlValue> list = new List<SqlValue>();
                                    list.Add(new SqlValue("Name", "'" + players[0].UserAccountName + "'"));
                                    list.Add(new SqlValue("CanChange", 1));
                                    list.Add(new SqlValue("R", text1));
                                    list.Add(new SqlValue("G", text2));
                                    list.Add(new SqlValue("B", text3));
                                    SQLEditor.InsertValues("ChatColor", list);

                                }

                            }
                            else
                            {

                                args.Player.SendMessage("Player " + text[3] + " needs to be logged in, or you need to type his/her account name.", System.Drawing.Color.Red); return;

                            }
                        }   

                    }
                args.Player.SendMessage("Text Color for " + text[3] + " successfully changed!", text1, text2, text3); return;
            }
            else
            {

                args.Player.SendMessage("Not enough arguments. Format is /otherchatcolor Red Green Blue Username", System.Drawing.Color.Red);
                return;

            }
        }

        public static int SearchTable(List<object> Table, string Query)
        {

            for (int i = 0; i < Table.Count; i++)
            {

                if (Query == Table[i].ToString())
                {

                    return (i);

                }

            }
            return (-1);

        }
    }
}