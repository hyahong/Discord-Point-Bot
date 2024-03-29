﻿using System;
using System.Data.SQLite;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace Discord_Point_Bot
{
    public class SQLite
    {
        private static SQLite instance = null;

        private SQLiteConnection connection;

        public static SQLite Instance()
        {
            if (instance == null)
                instance = new SQLite();
            return instance;
        }

        public static void Free()
        {
            if (instance != null)
                instance.Close();
        }

        SQLite()
        {
            try
            {
                connection = new SQLiteConnection("Data Source=local.db");
                connection.Open();
                Console.WriteLine("SQLite is connected");
                TabaleInit();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void Close()
        {
            connection.Close();
            Console.WriteLine("SQLite is disconnected");
        }

        private void TabaleInit()
        {
            SQLiteCommand command = new SQLiteCommand(
                "CREATE TABLE IF NOT EXISTS User (user TEXT, donate TEXT, attendance TEXT, point INTEGER)"
                , connection
                );
            command.ExecuteNonQuery();
            command = new SQLiteCommand(
                "CREATE TABLE IF NOT EXISTS Bet (title TEXT, date TEXT, event TEXT)"
                , connection
                );
            command.ExecuteNonQuery();

        }

        /*
         * Manage SQL Table 'User'
         * 
         */
        public void UserTableInsert(string id, string attendance, int point)
        {
            Console.WriteLine($"new row in User {id}");
            SQLiteCommand command = new SQLiteCommand(
                $"INSERT INTO User (user, donate, attendance, point) values ('{id}', '', '{attendance}', {point})",
                connection
                );
            command.ExecuteNonQuery();
        }

        public void UserUpdate(string id, string donate = null, string attendance = null, int point = -1)
        {
            string target = "";
            if (donate != null)
                target = $"donate='{donate}'";
            else if (attendance != null)
                target = $"attendance='{attendance}'";
            else if (point != -1)
                target = $"point={point}";
            SQLiteCommand command = new SQLiteCommand(
                $"UPDATE User SET {target} WHERE user={id}",
                connection
                );
            command.ExecuteNonQuery();
        }

        public User GetUser(string id)
        {
            Console.WriteLine($"get User {id}");
            User result = new User();
            SQLiteCommand cmd = new SQLiteCommand(
                $"SELECT * FROM User WHERE user='{id}'",
                connection
                );
            SQLiteDataReader reader = cmd.ExecuteReader();
            if (!reader.HasRows)
            {
                UserTableInsert(id, "null", 0);
                result.Donate = "";
                result.Attendance = "null";
                result.Point = 0;
                return result;
            }
            while (reader.Read())
            {
                result.Donate = reader["donate"].ToString();
                result.Attendance = reader["attendance"].ToString();
                result.Point = int.Parse(reader["point"].ToString());
            }
            reader.Close();
            return result;
        }

        /*
         * Manage SQL Table Bet
         * 
         */
        public void BetTableInsert(string title, string data)
        {
            Console.WriteLine($"new row in Bet {title}");
            SQLiteCommand command = new SQLiteCommand(
                $"INSERT INTO Bet (title, date, event) values ('{title}', '{DateTime.Now:MMddyyyyHHmmss}', '{data}')",
                connection
                );
            command.ExecuteNonQuery();
        }

        public List<Event> GetBetList()
        {
            List<Event> events = new List<Event>();
            Event block;
            JObject json;
            Form form;

            SQLiteCommand cmd = new SQLiteCommand(
                $"SELECT * FROM Bet",
                connection
                );
            SQLiteDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                block = new Event();
                json = JObject.Parse(reader["event"].ToString());
                block.title = json["title"].ToString();
                block.author = json["author"].ToString();
                block.date = json["date"].ToString();
                foreach (JToken token in json["events"].Children())
                {
                    form = new Form();
                    form.form = token["form"].ToString();
                    block.forms.Add(form);
                    foreach (JToken u in token["users"].Children())
                        form.users.Add(u.ToObject<BetUser>());
                }
                events.Add(block);
            }
            reader.Close();
            return events;
        }

        public void BetUpdate(string date, string data)
        {
            SQLiteCommand command = new SQLiteCommand(
                $"UPDATE Bet SET event='{data}' WHERE date='{date}'",
                connection
                );
            command.ExecuteNonQuery();
        }

        public void BetDelete(string date)
        {
            SQLiteCommand command = new SQLiteCommand(
                $"DELETE From Bet WHERE date='{date}'",
                connection
                );
            command.ExecuteNonQuery();
        }

    }
}
