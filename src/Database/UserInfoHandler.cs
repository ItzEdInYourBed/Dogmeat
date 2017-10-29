﻿using System;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Dogmeat.Database
{
    public class UserInfoHandler
    {
        public MySqlConnection Connection;

        public UserInfoHandler(MySqlConnection connection) => Connection = connection;
        
        public async Task AddUser(UUser User) => AddUser(User.ID, User.Experience, User.Description);

        public async Task AddUser(ulong ID, ushort Experience = 0, String Description = "none")
        {
            if (Description.Length > 30)
                throw new Exception("Description limit is thirty characters.");
            
            MySqlCommand Command = Connection.CreateCommand();
            Command.Parameters.AddWithValue("ID", ID);
            Command.Parameters.AddWithValue("Experience", Experience);
            Command.Parameters.AddWithValue("Description", Description);
            Command.CommandText = "INSERT INTO Users VALUES(@ID, @Experience, 0, 0, @Description)";

            await Utilities.MySql.ExecuteCommand(Connection, Command, Utilities.MySql.CommandExecuteType.NONQUERY);
        }

        public async Task<UUser> GetUser(ulong ID)
        {
            UUser User = new UUser(ID, 0, 0, 0, "");
            
            try
            {
                MySqlCommand Command = Connection.CreateCommand();
                Command.Parameters.AddWithValue("ID", ID);
                Command.CommandText = "SELECT * FROM Users WHERE ID = @ID";

                await Connection.OpenAsync();

                using (MySqlDataReader Reader = Command.ExecuteReader())
                {
                    while (await Reader.ReadAsync())
                    {
                        User.Experience = (ushort) Reader.GetInt16(1);
                        User.Level = (ushort) Reader.GetInt16(2);
                        User.Global = (uint) Reader.GetInt32(3);
                        User.Description = Reader.GetString(4);
                    }
                }
            }
            catch (Exception e) { Console.WriteLine(e); }
            finally { Connection.Close(); }

            return User;
        }

        public async Task<bool> CheckUser(ulong ID)
        {
            bool Exists = false;
            
            MySqlCommand Command = Connection.CreateCommand();
            Command.Parameters.AddWithValue("ID", ID);
            Command.CommandText = "SELECT EXISTS(SELECT 1 FROM Users WHERE ID = @ID LIMIT 1);";

            object Result =
                await Utilities.MySql.ExecuteCommand(Connection, Command, Utilities.MySql.CommandExecuteType.SCALAR);
            
            if (Result != null)
            {
                Int32.TryParse(Result.ToString(), out int exists);

                Exists = exists != 0;
            }
            
            return Exists;
        }
    }
}