// ICS 3U FSE (Server Code)
// Noor Nasri (In charge of server code, database, server/client communications, and some functions on the client side) 
// and Anish Aggarwal (In charge of client code, making the wanted design, displaying the game statuses, and all the other front end stuff)
// This is a real time multiplayer 3D strategy game. You will start in the same position as five others.
// Limited money and a negative income. You can only gain income by mining the crystals, but there's a limit to them!
// You can't leave the crystals unattended because well, anyone can put a building right there and kill your plants.
// You must slowly outresource your opponents, often with board tactics, so that you can strategically beat them. 
// Skill can be further shown in manuavers, as there are hidden tricks to the game :)


// ========================================= Importing anything required =========================================
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;
using System.Xml.Serialization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;

// ========================================= Defining Classes =========================================
public class Stringlify_Message : MessageBase
{ // This is the class we use to communicate between client and server. Being derived from Message base makes it auto serilized  
    public string main_message;
    public string reason;
    public string extra_info;
    public string extra_info2;
}

public class UnitInfo
{   // We store more specific information about the units in this class, because c# doesn't allow mixed lists but there is a variaty of data to store
    //Needed info
    public int MaxHp;
    public int special_char;
    public float spawn_delay;
    public float spawn_hover = 0; // how much above ground they spawn
    public int cost;

    //Needed Info, shortened by having defaults
    public float walk_speed = 0;
    public int shot_distance = -1;
    public Dictionary<string, int> heal_distance = new Dictionary<string, int> { { "Troops", -1 }, { "Planes", -1 },  {"Buildings", -1 } };

    public List<string> can_spawn = new List<string> { };
    public int income_change = 0;
    public int housing_change = 0;

    public bool can_float = false;
    public bool can_walk = false;
    public bool needs_energy = false; 

    //Info needed if default has been switched
    public int shoot_debounce;
    public float shot_speed = 3; 
    public int damage;
    public int heal_debounce;
    public string bullet_type;
    public int unit_space;

    public int unit_type = 0; // 1 is building, 0 is unit
    public int space_x = 1;
    public int space_z = 1;
        
    //Extra info
    public List<string> special_effects = new List<string> { };
}

public class Game_Mechanics // To be accessed by any other required classes, put here to not clog space
{
    public static Dictionary<string, int> shop_prices = new Dictionary<string, int> { { "Default", 0 } }; // Shop is a WIP

    //Round set ups
    public static List<List<int>> colour_setup = new List<List<int>> { new List<int> {1, 0, 0 }, new List<int> { 1, 0, 0 },
        new List<int> { 1, 0, 0 }, new List<int> {1, 0, 0 }, new List<int> {1, 0, 0 }, new List<int> {1, 0, 0 }};

    public static List<List<float>> position_setup = new List<List<float>> { new List<float> {1, 0, 0 }, new List<float> { 1, 0, 0 },
        new List<float> { 1, 0, 0 }, new List<float> {1, 0, 0 }, new List<float> {1, 0, 0 }, new List<float> {1, 0, 0 }};

    //Large unit data initilization
    public static List<string> UnitOrder = new List<string> { null, "Command Center", "Light Soldier", "Heavy Soldier", "Power Plant",
        "Plane", "House", "Head Quarters"}; // "Nuclear Plant", "Head Quarters", "House", "Airport", "Barracks"
    public static Dictionary<string, UnitInfo> UnitDetails = new Dictionary<string, UnitInfo> {
        {"Command Center", new UnitInfo{ MaxHp = 800, bullet_type = "Turret", can_walk = true,  unit_type = 1, spawn_delay = 10f, spawn_hover = 2, housing_change = 1,
            shoot_debounce = 4, damage = 40, shot_distance = 30, can_spawn = new List<string> { "Light Soldier","Heavy Soldier",  "Plane"}, space_x = 6, space_z = 6, special_char = 1, cost = 150 }  },
        {"Light Soldier", new UnitInfo{ MaxHp = 60, bullet_type = "Bullet", can_walk = true, damage = 10, shoot_debounce = 2, walk_speed = 2.5f, shot_distance = 15,
            special_char = 2, spawn_delay = 3f, cost = 20, unit_space = 1, spawn_hover = 1} },
        {"Heavy Soldier", new UnitInfo{ MaxHp = 100, bullet_type = "Bullet", can_walk = true, damage = 20, shoot_debounce = 2, walk_speed = 2.5f, shot_distance = 20,
            special_char = 3, spawn_delay = 2.5f, cost = 30, unit_space = 1, spawn_hover = 1 } },
        {"Power Plant", new UnitInfo{ MaxHp = 250, can_walk = true, spawn_delay = 1f, space_x = 5, space_z = 5, unit_type = 1, income_change = 10,
            needs_energy = true, special_char = 4, cost = 100, spawn_hover = 2 } },
        {"Plane", new UnitInfo{ MaxHp = 200, bullet_type = "Bullet", can_walk = true, damage = 50, shoot_debounce = 2, walk_speed = 4.5f, shot_distance = 25,
            special_char = 5, spawn_delay = 5.5f, cost = 400, unit_space = 1, spawn_hover = 1, can_float = true} },
        {"House", new UnitInfo{ MaxHp = 100, can_walk = true,  unit_type = 1, spawn_delay = 10f, spawn_hover = 1, space_x = 3, space_z = 3,
            special_char = 6, cost = 150, housing_change = 3 }  },
        { "Head Quarters", new UnitInfo{ MaxHp = 1500, bullet_type = "Turret", can_walk = true,  unit_type = 1, spawn_delay = 10f, spawn_hover = 3,
            shoot_debounce = 4, damage = 75, shot_distance = 45, space_x = 6, space_z = 6, special_char = 7, cost = 650 }  }
        
        /*{"Nuclear Plant", new UnitInfo{ MaxHp = 300, can_walk = true, spawn_delay = 1f, space_x = 5, space_z = 5, unit_type = 1, income_change = 15,
            needs_energy = true, special_char = 5, cost = 250, spawn_hover = 2 } },
        
        {"Airport", new UnitInfo{ MaxHp = 200, can_walk = true,  unit_type = 1, spawn_delay = 10f, spawn_hover = 2,
            can_spawn = new List<string> { "Plane" }, space_x = 4, space_z = 4, special_char = 8, cost = 800 }  },
        {"Barracks", new UnitInfo{ MaxHp = 100, can_walk = true,  unit_type = 1, spawn_delay = 10f, spawn_hover = 2,
            can_spawn = new List<string> { "Light Soldier" , "Heavy Soldier" }, space_x = 4, space_z = 4, special_char = 9, cost = 250 }  },
        */
    };

    public static string Random_str(int length)
    { // Will randomly generate a string with length "length". Useful for creating unique IDs
        string chars_allowed = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var stringChars = new char[length];
        var random = new System.Random();

        for (int i = 0; i < stringChars.Length; i++)
        {
            stringChars[i] = chars_allowed[random.Next(chars_allowed.Length)];
        }
        
        return new String(stringChars);
    }
    
}




public class Converter //For converting from object to another
{
    public static string JS_String(JSONNode item) //Converts  the JSONNode to a string
    {
        string new_item = item.ToString();
        return new_item.Substring(1, new_item.Length - 2);
    }

    public static List<string> JS_LoS(JSONNode item) //Converts it to a list of strings
    {
        List<string> new_item = new List<string> { };
        for (int i = 0; i < item.Count; i++)
        {
            new_item.Add(JS_String(item[i]));
        }
        return new_item;
    }

    public static List<List<int>> JS_LoLoN(JSONNode item) // Converts a 2d list
    {
        List<List<int>> full_item = new List<List<int>> { }; //create new one

        for (int i = 0; i < item.Count; i++)//loop through all
        {
            List<int> new_item = new List<int> { }; // make another empty list
            for (int a=0; a < item[i].Count; a++)
            {
                new_item.Add((int)item[i][a]); // add everything within that spot to the new_item list
            }
            full_item.Add(new_item); //update full_item with the new_item since its a 2d list
        }
        return full_item;
    }
    
    public static string Encode_XML(object obj_tohide, Type required_type)
    { // Given an object that can be serilized and the type it is
        XmlSerializer serializer = new XmlSerializer(required_type);
        StringWriter sw = new StringWriter();
        XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
        serializer.Serialize(sw, obj_tohide, ns);
        string converted_string = sw.ToString();

        return converted_string;
    }
    //We reduced XML decoding to 3 lines (Well, in terms of lines it could be just stuffed in 1) - but we could not make a function for it
    //Because we needed to send a class like List<int> as a parameter and use it as a variable which c# doesn't allow
}

public class Player_Connected //Class for every client
{
    // ----------------------------------- Defining variables for players -----------------------------------
    public NetworkConnection ClientConnect;
    public Room SelectedRoom;
    public Game CurrentGame;

    public string Username;
    public string Password;

    public int Tokens = 0; 
    public int Wins = 0;
    public int Losses = 0;
    public int Level = 1;
    public int XP = 0;

    public string PP = "Default"; // profile pic
    public string ST = "Default"; // selected trail for when playing the game

    public List<string> owned_pics = new List<string> { "Default" };
    public List<string> owned_trails = new List<string> { "Default"};
    public List<List<int>> owned_colours = new List<List<int>> { };
    public bool IsVip = false;

    // ----------------------------------- Defining methods for players -----------------------------------
    public IEnumerator Send_message(string r, string mm, string e1, string e2)
    { // sends the messages wanted as the stringlify message class accross the network
        Stringlify_Message client_mes = new Stringlify_Message
        {
            main_message = mm,
            reason = r,
            extra_info = e1,
            extra_info2 = e2
        };

        bool works = ClientConnect.SendByChannel(Server.chatMessage, client_mes, Server.channel_ID);
        
        yield return new WaitForSeconds(0.003f); // wait a little after every message
    }

    public IEnumerator Create_Account(bool new_account, string username, string password)
    {
        //Signs you in according to the name, changes the variables if needed, hits up client with new info

        WWWForm form = new WWWForm(); // making a request form
        if (new_account)
        {
            form.AddField("Request", "Register");
        }
        else
        {
            form.AddField("Request", "Sign in");
        }
        
        form.AddField("Username", username); //filling out parameters 
        form.AddField("Password", password);

        using (UnityWebRequest request_test = UnityWebRequest.Post("https://script.google.com/macros/s/AKfycbxpMFw4hI_kC4QdjghxmR8EBCk5C0U6GRH8N5Nl3f9YreopWg0/exec", form))
        {
            yield return request_test.SendWebRequest();

            string response_ret = request_test.downloadHandler.text;
            Debug.Log(response_ret);
            if (new_account) // They are registering
            {
                if (response_ret == "true") // It worked!
                {
                    Username = username;
                    Password = password;
                    Server.Add_Global(string.Format("  Please welcome {0} to the game!", Username));

                }
                //send them the message
                if (response_ret == "false" || response_ret == "true")
                {
                    yield return Send_message("Register response", response_ret, username, null);
                }
                else
                {
                    yield return Send_message("Register response", "DB", username, null);
                }
                
                
                
            }
            else // They are signing in
            {
                if (response_ret == "\"Username and Password do not match\"") //oh nope, they got it wrong!
                {
                    yield return Send_message("Login response", "false", null, null);
                }
                else //parse the json data and cast it to its proper  types, then inform the player.
                {
                    var results = JSON.Parse(response_ret); //parsing it with SimpleJSon
                    string final_message;

                    try
                    {
                        //All the strings
                        Username = Converter.JS_String(results["Username"]);
                        Password = Converter.JS_String(results["Password"]);
                        PP = Converter.JS_String(results["Profile Default"]);
                        ST = Converter.JS_String(results["Default Trail"]);

                        //All the ints
                        Tokens = (int)results["Tokens"];
                        Level = (int)results["Level"];
                        XP = (int)results["EXP"];
                        Wins = (int)results["Wins"];
                        Losses = (int)results["Losses"];

                        //The lists
                        owned_pics = Converter.JS_LoS(results["Profile Pics"]);
                        owned_trails = Converter.JS_LoS(results["Owned Items"]);
                        owned_colours = Converter.JS_LoLoN(results["Bonus Colours"]);

                        //Boolean
                        IsVip = results["VIP Status"].Value == "True";

                        //Encoded lists I send to the client so that they know the account info
                        List<string> final_uncoded = new List<string> {
                        Converter.Encode_XML(new List<string> { Username, Password, PP, ST}, typeof(List<string>)),
                        Converter.Encode_XML(new List<int> { Tokens, Level, XP, Wins, Losses }, typeof(List<int>)),
                        Converter.Encode_XML(IsVip, typeof(bool))};

                        final_message = Converter.Encode_XML(final_uncoded, typeof(List<string>));

                    }
                    catch
                    {
                        final_message = "Did not work";
                    }

                    if (final_message == "Did not work")
                    {
                        yield return Send_message("Login response", "DB", null, null);
                    }
                    else
                    {
                        yield return Send_message("Login response", "true", final_message, null); // tell them that login worked
                        Server.Add_Global(string.Format("  Welcome back, {0}!", Username));
                    }
                    

                }
            }
        }
    }
    

    public bool Purchase(string kind, string item)
    {
        //Check if you can purchase or not, makes the changes and returns true/false
        // WIP
        return false;
    }
}

// ----------------------------------- Game Class that manages itself -----------------------------------
public class Game
{
    public class GameTeam // For the alliances
    {
        public int ID; // the unique identifier for teams
        public List<GamePlayer> Team_members = new List<GamePlayer> { }; // helpful list to find the players
        public List<string> Team_messages = new List<string> { }; // team messages
        
        public IEnumerator Team_message(string mes_expected)
        { // Handle new team message and tell players of new messages
            Team_messages.Add(mes_expected);

            if (Team_messages.Count >= 30)
            { // Overlimit, toss one message out
                Team_messages = Team_messages.Skip(1).ToList();
            }
            string all_messages = Converter.Encode_XML(Team_messages, typeof(List<string>));
            foreach (GamePlayer plr in Team_members)
            {
                if (plr.IsConnected) 
                { //Inform all team members thta are still connected of this message
                    yield return plr.PlrObject.Send_message("Team Messages", all_messages, null, null);
                }
            }
        }
    }

    public class GamePlayer
    {
        //references
        public Player_Connected PlrObject;
        public List<Unit> owned_units = new List<Unit> { };
        public GameTeam Team;
        public int PlrIndex;
        public bool IsConnected = true;

        //Player data
        public List<float> CamPos; 
        public List<int> Colour;
        public float last_flush = 0;

        //Player stats
        public int cash = 500;
        public int income = -5;

        public int max_units = 10;
        public int max_buildings = 35;

        public int unit_count = 0;
        public int building_count = 0;

        public Unit CreateUnit(string unit_name) // returns a new unit with the correct id, made under this player
        {
            UnitInfo unit_detail = Game_Mechanics.UnitDetails[unit_name];
            string cool_ID = PlrIndex.ToString() + Team.ID.ToString() + unit_detail.special_char.ToString("D3") + ((int)Time.realtimeSinceStartup).ToString() + Game_Mechanics.Random_str(5);
            Unit new_unit = new Unit { specific_details = unit_detail, HP = unit_detail.MaxHp, unique_ID = cool_ID, unit_name = unit_name,
             unit_owner = Player_DB[PlrObject], unit_team = Team};
            new_unit.self = new_unit;

            return new_unit;
        }
        

        public List<string> private_messages = new List<string> { }; // storing PMs
        public IEnumerator Add_Private(string ex_mes)
        {   // Handles private messages. Client support was not added :/
            private_messages.Add(ex_mes);
            if (private_messages.Count > 25)
            {
                private_messages = private_messages.Skip(1).ToList();
            }
            string priv_lis = Converter.Encode_XML(private_messages, typeof(List<string>));
            yield return PlrObject.Send_message("Private Game Messages", priv_lis, null, null);
        }
        public IEnumerator Update_plr() //telling the player their stats again
        {
            List<int> sending_info = new List<int> { cash, income, max_units, max_buildings, unit_count, building_count };
            string sending_string = Converter.Encode_XML(sending_info, typeof(List<int>));
            yield return PlrObject.Send_message("Game Stats", sending_string, null, null);
        }
        
        //PLayer wants to buy a building
        public void BuyBuilding(string unit_name, Vector3 position)
        {
            //required info
            UnitInfo item_wanted = Game_Mechanics.UnitDetails[unit_name];
            Vector3 newposition = position;
            bool works = true;

            //check if newposition is on land or water
            bool is_water = CollideWater(newposition);
            
            Unit new_build = CreateUnit(unit_name); //make new unit
            if (is_water) // buildings don't float on water 
            {
                works = false;
            }
            else
            {
                //check if the newposition is colliding with something
                works = Check_Spot(newposition, new_build, false);
            }
            

            //Works, check if they can purchase
            if (works && cash >= item_wanted.cost && building_count+1 <= max_buildings)
            {
                //Take the costs
                cash -= item_wanted.cost;
                building_count++;

                //Move the new building
                new_build.position = newposition;
                Unit_DB[new_build.unique_ID] = new_build;

                //Increase the counters
                max_units += item_wanted.housing_change;
                income += item_wanted.income_change;
                Debug.Log("Building has been made?");
            }
            else
            {
                Debug.Log("Building did not go through"); 
            }
            
        }

        //They want to spawn a unit from a building
        public void BuyUnit(string unit_name, Unit spawner_unit)
        {
            UnitInfo item_wanted = Game_Mechanics.UnitDetails[unit_name];
            //Check legitmency of deal they want to make
            if (spawner_unit.specific_details.can_spawn.Contains(unit_name) && cash >= item_wanted.cost && unit_count + item_wanted.unit_space <= max_units)
            {
                //Charge them mulah and increase counters
                cash -= item_wanted.cost;
                unit_count += item_wanted.unit_space;

                //Make new LoadingUnit and prepare the unit portion of it
                LoadingUnit incoming_unit = new LoadingUnit { spawner = spawner_unit, main_unit = CreateUnit(unit_name) };
                spawner_unit.due_soon.Add(incoming_unit);

                //Add them to the debounces of items to be spawned
                if (spawner_unit.due_times.Count == 0)
                {
                    spawner_unit.due_times.Add(item_wanted.spawn_delay);
                }
                else
                {
                    spawner_unit.due_times.Add(spawner_unit.due_times[spawner_unit.due_times.Count - 1] + item_wanted.spawn_delay);
                }
            }
        }

    }

    public class LoadingUnit
    { // A simple class that is stored in intermission while becoming a unit
        public Unit spawner;
        public Unit main_unit;

    }
     
    public class Unit
    { // The units of the game. Without going into specifics directly, holds indivisual data and hides behind the unique ID
        public GamePlayer unit_owner;
        public GameTeam unit_team;
        public UnitInfo specific_details;
        public Unit self; 

        public bool alive = true;
            
        public string unit_name; // We use this name to figure out all the stats (Speed, Max HP, Heal, Special Power, Debounces, swim/fly, Etc.)
        public string unique_ID;
        
        // For buildings spawning troops
        public List<LoadingUnit> due_soon = new List<LoadingUnit> { };
        public List<float> due_times = new List<float> { };
        public float last_run = Time.realtimeSinceStartup;

        //For moving
        public Vector3 position; 
        public Vector3 pos_target = null_vector; // same z as position so nothing flies
        
        //For shooting
        public Unit shooting_target = null;
        public int HP;

        public Shot current_bullet;

        //Debounces
        Dictionary<string, float> debounces = new Dictionary<string, float> { }; 
        
        public void Update_Unit()
        {//Puts the unit one frame into the future. This includes, spawning, shooting, dying, etc (not including moving, already done).
            float time_since = Time.realtimeSinceStartup - last_run;
            last_run = Time.realtimeSinceStartup;
            
            //consider losing target
            if (shooting_target != null)
            {
                if (Vector3.Distance(position, shooting_target.position) > specific_details.shot_distance || !shooting_target.alive)
                {
                    shooting_target = null;
                }
            }

            //find target
            if (specific_details.shot_distance > -1 && shooting_target == null && current_bullet == null) // they need a new target
            {
                Unit chosen_unit = null;
                float smallest_dist = specific_details.shot_distance;

                foreach (Unit current_unit in Units_In_Range(position, specific_details.shot_distance))
                { // if they are closer than anyone else in range
                    if (current_unit.unit_team.ID != unit_team.ID && Vector3.Distance(current_unit.position, position) < smallest_dist){
                        chosen_unit = current_unit;
                    }
                } 

                if (chosen_unit != null) // if someone is in range, start shooting
                {
                    shooting_target = chosen_unit;
                    debounces["Shooting"] = 0;
                }
            }

            //shoot new bullet
            if (shooting_target != null && current_bullet == null && last_run - debounces["Shooting"] > specific_details.shoot_debounce)
            {
                debounces["Shooting"] = last_run;
                current_bullet = new Shot
                {
                    target = shooting_target,
                    start_position = position,
                    positionB = new Vector3(position.x, position.y +specific_details.spawn_hover , position.z),
                    shooter = self
                };
            }

            //move old bullet
            if (current_bullet != null)
            {
                //Move bullet and check for hit
                bool finished = current_bullet.Move_bullet();        
                if (finished || !current_bullet.target.alive)
                {
                    current_bullet = null;
                }
            }

            //get closer to spawning items
            for(int i=0; i<due_times.Count;i++)
            {
                due_times[i] -= time_since;
            }

            //check if item should be spawned
            if (alive && due_times.Count>0 && due_times[0] < 0)
            {//Spawn the unit on due_soon[0]
                List<List<float>> possible_changes = new List<List<float>> { new List<float> { 6/5, 0 }, new List<float> {-6 / 5, 0 }, new List<float> {0, 6/5 },
                new List<float> { 0, -6/5}, new List<float> { 6 / 5, 6/5}, new List<float> { 6 / 5, -6/5 }, new List<float> { -6 / 5, -6/5}, new List<float> {-6 / 5, 6 / 5 },
                new List<float> { 8/7, 0 }, new List<float> {-8 / 7, 0 }, new List<float> {0, 8/7 }, new List<float> { 0, -8/7},
                new List<float> { 8 / 7, 6/5}, new List<float> { 8 / 7, -8/7}, new List<float> { -8 / 7, -8/7}, new List<float> {-8 / 7, 8 / 7 } };
                
                //Check_Spot
                Vector3 working_pos = new Vector3();
                var s = due_soon[0].spawner;
                var w = due_soon[0].main_unit;

                bool can_fly = w.specific_details.can_float && w.specific_details.can_walk;
                
                // Try each spot until one of them works, or wait it out.
                foreach (List<float> change in possible_changes) // find a good place to spawn it
                {
                    working_pos = new Vector3(s.position.x + (s.specific_details.space_x * change[0]),
                    s.position.y, s.position.z + (s.specific_details.space_z * change[1]) );

                    if (can_fly) // spawn it above current item
                    {
                        working_pos.y += 8;
                    }

                    bool works = Check_Spot(working_pos, s, can_fly); // check if new spot works out
                    if (works)
                    {// make the unit
                        due_soon[0].main_unit.position = working_pos;
                        Unit_DB[due_soon[0].main_unit.unique_ID] = due_soon[0].main_unit;

                        //remove from queue
                        due_soon = due_soon.Skip(1).ToList();
                        due_times = due_times.Skip(1).ToList();
                        break;
                    }
                }

                
            }
        }
        
    }

    public class Shot
    { // The shot class in the game
        public Vector3 start_position;
        public Vector3 positionB;

        public Unit target;
        public Unit shooter;

        public bool Move_bullet()
        {
            //moves the bullet, checks for collision with target
            Vector3 new_pos = new Vector3(target.position.x, target.position.y + target.specific_details.spawn_hover, target.position.z);

            positionB = Vector3.MoveTowards(positionB, new_pos, shooter.specific_details.shot_speed);
            
            if (Vector3.Distance(positionB, new_pos) < 2)
            { 
                target.HP -= shooter.specific_details.damage;
                if (target.HP <= 0) // unit killed
                {
                    Remove_unit(target.unique_ID);                    
                }
                return true;
            }
            //return true if it goes away
            return false;
        }
    }

    
    // ----------------------------------- Defining variables for games -----------------------------------
    public bool started = false;
    

    public Dictionary<string, Transform> simulated_units = new Dictionary<string, Transform> { };

    public List<string> global_messages = new List<string> { };

    public List<GameTeam> Team_DB = new List<GameTeam> { };
    public static Dictionary<Player_Connected, GamePlayer> Player_DB = new Dictionary<Player_Connected, GamePlayer> { };

    public static Dictionary<string, Unit> Unit_DB = new Dictionary<string, Unit> { }; // For quickly making changes to Units (Sell, Change intention, etc.)
    
    public string map;
    public float last_income = 0;
    
    public static List<List<List<Unit>>> position_graph = new List<List<List<Unit>>> { }; //700, 750 --> 1450, 1250

    public static Vector3 null_vector = new Vector3(100000, 100000, 100000); // A vector3 cannot be set to null, so this is our replacement

    // ----------------------------------- Defining methods for games -----------------------------------
    int connect_time = 0;
    public IEnumerator Update_frame() // calls all the functions that do things
    {//move everyone, fire the bullets, remove units, add units, etc.
        started = false;
        
        // pre-adjustments loop
        List<List<float>> plr_cams = new List<List<float>> { };

        bool bonus_time = false;
        if (Time.realtimeSinceStartup - last_income > 6) // time for the income update
        {
            last_income = Time.realtimeSinceStartup;
            bonus_time = true;
        }

        foreach (KeyValuePair<Player_Connected, GamePlayer> dict_plr in Player_DB)
        {
            if (bonus_time && dict_plr.Value.max_buildings > 0) // still alive && income time
            {
                dict_plr.Value.cash = Math.Max(0, dict_plr.Value.cash + dict_plr.Value.income);
                yield return dict_plr.Value.Update_plr();

                if (dict_plr.Value.cash == 0 && dict_plr.Value.unit_count == 0 && dict_plr.Value.building_count == 0)
                {
                    yield return Remove_Plr(dict_plr.Value.PlrObject, 2);
                }
            }
            plr_cams.Add(dict_plr.Value.CamPos); // Add the campos into our list of positions that the client gets
        }

        //700-->1450, 550 -->1250
        List<List<List<Unit>>> current_positions = new List<List<List<Unit>>> { };
        for (int a = 700; a <= 1450; a = a + 10) // recreate current_positions with full lists
        {
            List<List<Unit>> FirstDim = new List<List<Unit>> { };
            for (int b = 550; b <= 1250; b = b + 10)
            {
                FirstDim.Add(new List<Unit> { });
            }
            current_positions.Add(FirstDim);
        }

        //Update the positions and place them in boxes according to position to avoid having to iterate through everyone for collisions
        foreach (KeyValuePair<string, Unit> current_unit in Unit_DB)
        {
            if (current_unit.Value.pos_target != null_vector) // they want to move somewhere
            {
                bool is_flying = (current_unit.Value.specific_details.can_walk == current_unit.Value.specific_details.can_float == true);
                int num_iter = (int)(current_unit.Value.specific_details.walk_speed / 0.5f);
                for (int ex_move =0; ex_move<= num_iter; ex_move++) // Transition them in steps in case there are tiny obstacles
                {
                    Vector3 newposition = Vector3.MoveTowards(current_unit.Value.position, current_unit.Value.pos_target, current_unit.Value.specific_details.walk_speed/num_iter);
                    if (!current_unit.Value.specific_details.can_float) // walking units must not float
                    { // shoot a ray downwards and find how far off the ground they are, then calculate their newposition accordingly
                        RaycastHit hit;;
                        Ray ray = new Ray(new Vector3(newposition.x,newposition.y+0.1f,newposition.z), Vector3.down);
                        if (Physics.Raycast(ray, out hit)) { }
                        newposition.y -= (hit.distance-0.1f);
                    }

                    bool works = Check_Spot(newposition, current_unit.Value, is_flying);
                    
                    if (works) // the new position works!
                    {
                        current_unit.Value.position = newposition;
                        if (newposition == current_unit.Value.pos_target) // reached target
                        {
                            current_unit.Value.pos_target = null_vector;
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

            }// the map is contained within those values: x: 700-->1450, z: 550 -->1250
            current_positions[(int)(current_unit.Value.position.x - 700) / 10][(int)(current_unit.Value.position.z - 550) / 10].Add(current_unit.Value);
        }


        position_graph = current_positions; // the positions have been updated

        List<string> unique_IDS = new List<string> { };
        List<List<float>> unit_info = new List<List<float>> { }; // [x, y, z, hp, 1 if bullet or 0 if not, bullet x, bullet y, bullet z, health]

        //Loop through units. The update unit will check for Shooting; Collision; Spawning
        List<string> cur_lis =  Unit_DB.Keys.ToList();
        for (int a = cur_lis.Count-1; a >= 0; a--)
        {
            try
            {
                KeyValuePair<string, Unit> current_unit = new KeyValuePair<string, Unit>(cur_lis[a], Unit_DB[cur_lis[a]]);

                current_unit.Value.Update_Unit(); // Let the unit handle itself

                if (current_unit.Value.alive) // updating the list we send clients
                {
                    unique_IDS.Add(current_unit.Value.unique_ID); // updating ID
                    if (current_unit.Value.current_bullet == null) // updating unit with no bullet
                    {
                        unit_info.Add(new List<float> { current_unit.Value.position.x, current_unit.Value.position.y,
                    current_unit.Value.position.z, current_unit.Value.HP});
                    }
                    else // updating unit with bullet
                    {
                        unit_info.Add(new List<float> { current_unit.Value.position.x, current_unit.Value.position.y,
                    current_unit.Value.position.z, 1, current_unit.Value.current_bullet.positionB.x,
                        current_unit.Value.current_bullet.positionB.y,
                    current_unit.Value.current_bullet.positionB.z, current_unit.Value.HP});
                    }
                }
            }
            catch
            {
                Debug.Log("Some unit was probably removed at a bad time");
            }
            
        }


        //map everything and send to all clients in game
        string encoded_IDS = Converter.Encode_XML(unique_IDS, typeof(List<string>));
        string pos_string = Converter.Encode_XML(unit_info, typeof(List<List<float>>));
        string cam_string = Converter.Encode_XML(plr_cams, typeof(List<List<float>>));
        
        foreach (KeyValuePair<Player_Connected, GamePlayer> current_plr in Player_DB)
        {
            if (current_plr.Value.IsConnected) // if they still want the connections
            {
                yield return current_plr.Key.Send_message("Game State", encoded_IDS, pos_string, cam_string);
                if (Time.realtimeSinceStartup - current_plr.Value.last_flush > 10)
                {
                    //remove existing messages every 10 seconds
                    //current_plr.Key.ClientConnect.FlushChannels(); 
                }

            }
        }
        yield return new WaitForSeconds(0.25f);
        started = true; // ready to update the data again
    }

    public static bool CollideWater(Vector3 pos)
    { // this functions finds if the position is on top of land or water by shooting a raycast downwards
        RaycastHit hit;
        pos = new Vector3(pos.x, pos.y + 20, pos.z);
        Ray ray = new Ray(pos, Vector3.down);
        if (Physics.Raycast(ray, out hit)) { }
        return (hit.collider.name == "WaterCOLLISION");
    }

    public static bool Check_Spot(Vector3 newposition, Unit current_unit, bool is_flying) // returns if the unit can be there or not
    {
        //check if newposition is on land or water
        bool is_water = CollideWater(newposition);
        if (is_water && !current_unit.specific_details.can_float || !is_water && !current_unit.specific_details.can_walk)
        {
            return false;
        }
        
        //check if the newposition goes into a box that already had someone last time (position_graph)
        foreach (Unit possible_unit in Units_In_Range(newposition, 30))
        {
            if (possible_unit.unique_ID != current_unit.unique_ID) // if this is not the same unit
            { 
                //define variables needed for the calculation
                float del_x = Math.Abs(possible_unit.position.x - newposition.x);
                float del_z = Math.Abs(possible_unit.position.z - newposition.z);
                bool is_flying2 = (possible_unit.specific_details.can_walk == possible_unit.specific_details.can_float == true);

                if (is_flying == is_flying2 && ((del_x < (possible_unit.specific_details.space_x / 2 + current_unit.specific_details.space_x / 2)) &&
                    (del_z < (possible_unit.specific_details.space_z / 2 + current_unit.specific_details.space_z / 2))))
                {
                    return false;
                }
            }
        }

        return true; // none of the other checks took place, it can move there!
    }
    public static List<Unit> Units_In_Range(Vector3 position, float distance) // given the point and distance, it finds any units in nearby boxes
    {
        List<Unit> Close_Items = new List<Unit> { };
        distance = Math.Abs(distance);
        //Loop through every box in range
        for (int a=(int)(position.x-distance/2)/10; a<= (position.x+distance/2)/10; a++)
        {
            for (int b = (int)(position.z - distance / 2) / 10; b <= (position.z + distance / 2) / 10; b++)
            { //add everything within the boxes to the list
                foreach (Unit extra_unit in position_graph[a-70][b-55])
                {
                    Close_Items.Add(extra_unit);
                }
            }
        }

        return Close_Items;
    }

    public static void Remove_unit(string ID) // Called to remove units
    {
        Unit wanted_removal = Unit_DB[ID];

        //killing the item
        wanted_removal.alive = false;
        Unit_DB.Remove(wanted_removal.unique_ID);

        //reverting stat changes
        UnitInfo wanted_info = wanted_removal.specific_details;
        wanted_removal.unit_owner.income -= wanted_info.income_change;
        wanted_removal.unit_owner.max_units -= wanted_info.housing_change;

        //reverting spacing changes
        if (wanted_info.unit_type == 0)
        {
            wanted_removal.unit_owner.unit_count -= wanted_info.unit_space;
        }
        else
        {
            wanted_removal.unit_owner.building_count -= 1;
        }
        
        //remove items that are being spawned
        foreach (LoadingUnit incoming in wanted_removal.due_soon)
        {
            Remove_unit(incoming.main_unit.unit_name);
        }
    }

    public IEnumerator Order_given(Player_Connected plr, string command, List<string> selected_units, object extra_info) // execute the change a client wants
    {
        //check for order given 
        if (command == "Move")
        {
            var required_info = (Vector3)extra_info;
            foreach (string unit_id in selected_units)
            {
                Vector3 final_pos = required_info;
                if (!Unit_DB[unit_id].specific_details.can_float && final_pos.y < 1009.437)
                { // troops clicking on water
                    final_pos.y = 1009.437f;
                }
                else if (Unit_DB[unit_id].specific_details.can_float && Unit_DB[unit_id].specific_details.can_walk)
                { // planes
                    final_pos.y = Unit_DB[unit_id].position.y;
                }

                Unit_DB[unit_id].pos_target = final_pos; 
            }
        } else if (command == "Sell") 
        { // remove all the units they wish to sell and then give them money equal to the return cost (Up to half of original cost)
            foreach (string unit_id in selected_units)
            {
                Debug.Log(unit_id);
                if (Unit_DB[unit_id].unit_owner == Player_DB[plr])
                {
                    float ret_cost = Unit_DB[unit_id].specific_details.cost * Unit_DB[unit_id].HP / Unit_DB[unit_id].specific_details.MaxHp / 2;
                    Remove_unit(unit_id);
                    Player_DB[plr].cash += (int)ret_cost;
                    yield return Player_DB[plr].Update_plr();
                }
            }
        
        } else if (command == "Build")
        {
            Player_DB[plr].BuyBuilding(selected_units[0], (Vector3)extra_info);
        } else if (command == "Buy")
        { // [name of item wanted, ID of existing spawner]
            Player_DB[plr].BuyUnit(selected_units[0], Unit_DB[selected_units[1]]);
            yield return Player_DB[plr].Update_plr();
        }
    }

    public IEnumerator Start_Game(List<Player_Connected> players, string mode, string map)
    {   //Initialize everything
        Debug.Log("Game Started");

        GameTeam current_team = new GameTeam { ID = 0};
        int i = 0;
        List<string> game_plrs = new List<string> { }; // update those
        List<int> plr_teams = new List<int> { };
        
        foreach (Player_Connected current_plr in players)
        {
            // making new team if needed to
            if (mode == "FFA")
            {
                current_team = new GameTeam { ID = i };
                Team_DB.Add(current_team);
            } else if (mode == "3v3" && i == 3)
            {
                current_team = new GameTeam { ID = 1 };
            }
            else if (mode == "2v2v2" && i == 2)
            {
                current_team = new GameTeam { ID = 1 };
            }
            else if (mode == "2v2v2" && i == 4)
            {
                current_team = new GameTeam { ID = 2 };
            }
            game_plrs.Add(current_plr.Username);
            plr_teams.Add(current_team.ID);

            // telling each player what they are
            List<int> chosen_colour = Game_Mechanics.colour_setup[i];
            List<float> chosen_position = Game_Mechanics.position_setup[i];
            
            //Initilizing the player object
            GamePlayer plr_obj = new GamePlayer { CamPos = chosen_position, Colour = chosen_colour, PlrObject = current_plr, Team = current_team, PlrIndex = i};
            current_team.Team_members.Add(plr_obj);
            Player_DB[current_plr] = plr_obj;

            //Move to next plr
            i++;
        }

        // tell all the players the list of everything
        string playerList = Converter.Encode_XML(game_plrs, typeof(List<string>));
        string teamList = Converter.Encode_XML(plr_teams, typeof(List<int>));

        foreach (Player_Connected current_plr in players)
        {
            yield return current_plr.Send_message("Game Started", playerList, teamList, null);
        }

        //700-->1450, 550 -->1250
        for (int a=700; a<=1450; a=a+10)
        {
            List<List<Unit>> FirstDim = new List<List<Unit>> { };
            for (int b = 550; b <= 1250; b = b + 10)
            {
                FirstDim.Add(new List<Unit> { });
            }
            position_graph.Add(FirstDim);
        }
        
        started = true;

    }
    

    public IEnumerator New_Chat(string message, string channel, Player_Connected source, string receiver_name)
    {
        //For global game messages
        if (channel == "Global")
        {
            global_messages.Add(message);

            if (global_messages.Count >= 30)
            {
                global_messages = global_messages.Skip(1).ToList();
            }
            string all_messages = Converter.Encode_XML(global_messages, typeof(List<string>));
            foreach (KeyValuePair<Player_Connected, GamePlayer> plr_dict in Player_DB)
            {
                if (plr_dict.Value.IsConnected)
                {
                    yield return plr_dict.Key.Send_message("Global Game Messages", all_messages, null, null);
                }
            }
        }
        else if (channel == "Team")
        { // plr param is the sender
            yield return Player_DB[source].Team.Team_message(message);
        }
        else if (channel == "Private")
        { //private channels weren't added on the client side, so this is useless :/
            //find which player it is meant for
            GamePlayer receiver = null; 
            foreach (KeyValuePair<Player_Connected, Game.GamePlayer> pos_plr in Player_DB)
            {
                if (pos_plr.Key.Username == receiver_name)
                {
                    receiver = pos_plr.Value;
                    break;
                }
            }
            if (receiver != null) // if they gave us a username that is in game
            {
                if (receiver.IsConnected) // if they are still connected to the game
                {
                    yield return Player_DB[receiver.PlrObject].Add_Private(string.Format("{0}{1}: {2}", Player_DB[source].PlrIndex, source.Username, message));
                    yield return Player_DB[source].Add_Private(string.Format("{0}To {1}: {2}", receiver.PlrIndex ,receiver.PlrObject.Username, message));
                }
                else
                {
                    yield return Player_DB[source].Add_Private(string.Format("{0}{1} is not in the game", receiver.PlrIndex, receiver.PlrObject.Username));
                }
            }
            
        }
    }

    public IEnumerator Remove_Plr(Player_Connected PlrToKill, int reason) //remove player from the game
    {
        GamePlayer game_plr = Player_DB[PlrToKill];
        if (reason == 0) //player is gone
        {
            game_plr.IsConnected = false;
        }
        //reset needed items
        game_plr.max_buildings = 0;
        game_plr.max_units = 0;
        game_plr.cash = 0;
        game_plr.owned_units = new List<Unit> { };
        game_plr.unit_count = 0;
        game_plr.building_count = 0;

        //tell everyone about it
        string extra_str = new List<string> { "disconnected from the game room", "forfiet the game", "been killed" }[reason];
       
        yield return New_Chat(string.Format(" {0} has {1} ", PlrToKill.Username, extra_str), "Main", null, null);

        List<GameTeam> remaining = new List<GameTeam> { };
        foreach (KeyValuePair<Player_Connected,GamePlayer> plr in Player_DB)
        {
            if (plr.Value.max_buildings > 0 && !remaining.Contains(plr.Value.Team))
            {
                remaining.Add(plr.Value.Team);
            }
        }
        //if someone won!
        if (remaining.Count == 1)
        {
            yield return End_Game(remaining[0]); // end the game
        }
    }

    public void Move_cam(Player_Connected sender, List<float> new_pos)
    {
        Player_DB[sender].CamPos = new_pos; // calling it from main Server class won't work because access levels to Player_DB
    }

    public IEnumerator End_Game(GameTeam winner)
    {
        Debug.Log("Game is ending");
        Server.running_games.Remove(winner.Team_members[0].PlrObject.CurrentGame); //remove the game from being further updated
        //inform anyone still connected about the results
        foreach (KeyValuePair<Player_Connected, GamePlayer> plr in Player_DB)
        {
            if (plr.Value.IsConnected)
            {
                // tell them if they won or lost
                if (plr.Value.Team == winner)
                {
                    yield return plr.Key.Send_message("Game Ended", "Win", null, null);
                }
                else
                {
                    yield return plr.Key.Send_message("Game Ended", "Lose", null, null);
                }
            }
            plr.Key.CurrentGame = null; // reset the player's game variable
        }
        
    }
}

public class Room
{
    // ----------------------------------- Defining variables for rooms -----------------------------------
    public string map = "Default";
    public string room_name;
    public string chosen_map;
    public string mode;
    public string password;
    public Player_Connected creator;

    public List<Player_Connected> room_players = null;

    public List<string> local_chat = new List<string> { };

    // ----------------------------------- Defining methods for rooms called from the functions below them -----------------------------------
    public IEnumerator UpdateQueue() //Refresh the contents of room and tlel everyone
    {
        List<List<string>> que_info = new List<List<string>> { }; //Make new list
        //Go through all the players inside the room and add them to our list
        foreach (Player_Connected current_player in room_players)
        {
            que_info.Add(new List<string> { current_player.Username, current_player.PP });
        }
        string Queue_message = Converter.Encode_XML(que_info, typeof(List<List<string>>)); // encode it in XML

        //tell them all the list of players in the queue
        foreach (Player_Connected current_player in room_players)
        {
            yield return current_player.Send_message("Queue Update", Queue_message, null, null);
        }
    }

    public IEnumerator Destroy() // the owner has left, destroy the room
    {
        foreach (Player_Connected current_player in room_players)
        { //tell everyone
            current_player.SelectedRoom = null;
            yield return current_player.Send_message("Room removed", null, null, null);
        }

        foreach (KeyValuePair<string, Room> entry in Server.open_rooms) // finding the room object and removing it
        { // remove the room from existence
            if (entry.Value.room_players.Contains(creator))
            {
                Server.open_rooms.Remove(room_name);
                break;
            }
        }

    }

    public IEnumerator StartGame()
    { //Starting the game
        Game new_game = new Game();
        yield return new_game.Start_Game(room_players, mode, map);
        foreach (Player_Connected cur_plr in room_players)
        {
            cur_plr.CurrentGame = new_game;
        }
        yield return Destroy(); //destroy room object
        Server.running_games.Add(new_game); //Add the new room to the list of existing games
                
    }

    // ----------------------------------- Defining methods for rooms called from clients -----------------------------------
    public IEnumerator Join(Player_Connected plr) {
        room_players.Add(plr);

        if (room_players.Count == 6)
        {
            yield return StartGame();
        }
        else // tell everyone about the new people
        {
            yield return UpdateQueue();
        }

    }

    public IEnumerator Remove_plr(Player_Connected plr)  //someone wants to leave the queue
    {
        //check if the person is actually in the queue
        if (room_players.Contains(plr)){
            if (plr == creator) // if it's the creator, destroy the room
            {
                yield return Destroy();
            }
            else // if not, remove the player and update the room
            {
                room_players.Remove(plr);
                plr.SelectedRoom = null;
                yield return UpdateQueue();
            }
            
        }
    }
    
    
}


// ========================================= Starting Server =========================================
public class Server : MonoBehaviour
{
    // ================================ Defining variables ================================================
    int plr_count = 0;
    public const short chatMessage = 131;
    public const short EnterEvent = MsgType.Connect;
    public const short LeaveEvent = MsgType.Disconnect;
    public static int channel_ID; 

    public static Dictionary<NetworkConnection, Player_Connected> players_DB = new Dictionary<NetworkConnection, Player_Connected> { };

    public static Dictionary<string, Room> open_rooms = new Dictionary<string, Room> { }; // Index is room name
    public static List<Game> running_games = new List<Game> { };

    public static List<string> GlobalMessages = new List<string> { };
    
    public GameObject crystal_folder;
    
    // ================================ Setting up game ================================================
    public string LocalIPAddress() 
    { //this function retrieves their ip.
        IPHostEntry host;
        host = Dns.GetHostEntry(Dns.GetHostName()); // get the host
        //loop through all the addresses until we get the correct one
        foreach (IPAddress ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
                
            }
        }
        return null; // nothing was found
    }

    public IEnumerator SetUp_Server() // make the server 
    {
        //First, post our ip online so that other clients can find and connect to it
        string nameKeep = "https://script.google.com/macros/s/AKfycbyHApaejkQCI43E6QinTT3Pc1RGHGxh4Cgfm3Da2YuHvApbBlY/exec";

        string ip_s = LocalIPAddress();
        WWWForm form = new WWWForm(); // make a new request form to send it to sheets
        form.AddField("Request", "Set");
        form.AddField("IP", ip_s);


        using (UnityWebRequest request_test = UnityWebRequest.Post(nameKeep, form))
        {
            yield return request_test.SendWebRequest();
            // when the request is finished, open the server

            string response_ret = request_test.downloadHandler.text;

            ConnectionConfig myConfig = new ConnectionConfig();
            var new_id = myConfig.AddChannel(QosType.ReliableFragmented);
            channel_ID = (int)new_id;
            myConfig.PacketSize = 1999;
            myConfig.FragmentSize = 1500;
            
            myConfig.MaxSentMessageQueueSize = 200;
            NetworkServer.Configure(myConfig, 100);
            NetworkServer.maxDelay = 0.1f;

            NetworkServer.Listen(ip_s, 8362);

            // set our functions to activate on certain events
            NetworkServer.RegisterHandler(chatMessage, ServerReceiveMessage); // connect client messages to us
            NetworkServer.RegisterHandler(EnterEvent, OnClientAdd);
            NetworkServer.RegisterHandler(LeaveEvent, OnClientRemove);
            //NetworkServer.RegisterHandler(MsgType.Connect, OnConnected);
            Debug.Log("Server Started");
        }
    }

    void Start() // Server just started
    {
        float server_begin = Time.realtimeSinceStartup;
        Debug.Log("Starting Code");
        StartCoroutine(SetUp_Server());

    }


    // ================================ Updating all the games ================================================
    void Update()
    {
        //goes through all game instances and tells them to update frame
        foreach (Game current_game in running_games)
        {
            if (current_game.started)
            {
                StartCoroutine(current_game.Update_frame());
            }
        }
    }

    


    // ================================ Common Message Handeling Methods ================================================
    public static void Add_Global(string wanted_message)
    { // adds it to the list, and rmeoves one if there's too many. Much like global game messages
        GlobalMessages.Add(wanted_message);

        if (GlobalMessages.Count >= 30)
        {
            GlobalMessages = GlobalMessages.Skip(1).ToList();
        }
        string all_messages = Converter.Encode_XML(GlobalMessages, typeof(List<string>));

        Stringlify_Message client_mes = new Stringlify_Message { reason = "World Messages", main_message = all_messages, extra_info = null, extra_info2 = null };
        bool works = NetworkServer.SendByChannelToAll(chatMessage, client_mes, channel_ID); // tell all the clients the messages
    }

    public void Refresh_rooms(Player_Connected plr_object) // they want to get a list of all the running rooms
    {
        //Put all the room info into a list of strings, and send all the lists over
        List<List<string>> encoming_mes = new List<List<string>> { };
        foreach (KeyValuePair<string, Room> entry in open_rooms)
        {
            encoming_mes.Add(new List<string> {
                entry.Value.room_name,
                entry.Value.password,
                entry.Value.creator.Username,
                entry.Value.room_players.Count.ToString(),
                entry.Value.mode});
        }
        string return_mes = Converter.Encode_XML(encoming_mes, typeof(List<List<string>>)); // encode it
        StartCoroutine(plr_object.Send_message("Room Refresh", return_mes, null, null)); //send it to the player asking for it
    }

    // ================================ Handeling messaging ================================================
    private void ServerReceiveMessage(NetworkMessage networked_message)
    {
        // one of the clients has sent us a message
        NetworkConnection sender = networked_message.conn;
        Player_Connected plr_object = players_DB[sender];

        var message = networked_message.ReadMessage<Stringlify_Message>();
        Debug.Log(message.reason);

        // registration
        if (message.reason == "Register" || message.reason == "Sign In")
        {
            StartCoroutine(plr_object.Create_Account(message.reason == "Register", message.main_message, message.extra_info));
            
        }

        // matchmaking
        else if (message.reason == "Create Room")
        {
            Room variable;
            if (open_rooms.TryGetValue(message.main_message, out variable)) // The room already exists
            {
                StartCoroutine(plr_object.Send_message("Room Response", "false", null, null));
            }
            else 
            { // Set up the room according to the parameters they sent us
                Room new_room = new Room
                {
                    chosen_map = "Default",
                    creator = plr_object,
                    room_name = message.main_message,
                    password = message.extra_info,
                    mode = message.extra_info2,
                    room_players = new List<Player_Connected> { plr_object }
                };

                //make the room known to the server
                open_rooms[message.main_message] = new_room;
                plr_object.SelectedRoom = new_room;
                StartCoroutine(plr_object.Send_message("Room Response", "true", null, null)); // tell them it worked
            }
        }
        else if (message.reason == "Join Room")
        {
            
            Room variable;
            if (open_rooms.TryGetValue(message.main_message, out variable))
            {// room exists
                if (open_rooms[message.main_message].password == message.extra_info)
                {// room password is true
                    plr_object.SelectedRoom = open_rooms[message.main_message];
                    StartCoroutine(open_rooms[message.main_message].Join(plr_object));
                }
            }
            else
            { //they are outdated, update them on the rooms
                Refresh_rooms(plr_object);
            }
        }
        else if (message.reason == "Room List") // they want the list of rooms
        {
            Refresh_rooms(plr_object);
        }
        else if (message.reason == "Leave Room") // They want to leave their room
        {
            StartCoroutine(plr_object.SelectedRoom.Remove_plr(plr_object));
        }

        //chat
        else if (message.reason == "Global Chat")
        {
            Add_Global(string.Format("{0}: {1}", plr_object.Username, message.main_message));
            
            
        }else if (message.reason == "Queue Chat") // they want to talk with the others waiting in queue
        {
            //add the chat
            plr_object.SelectedRoom.local_chat.Add(string.Format("{0}: {1}", plr_object.Username, message.main_message));
            
            //check if there is too many messages
            if (plr_object.SelectedRoom.local_chat.Count >= 30)
            {
                plr_object.SelectedRoom.local_chat = plr_object.SelectedRoom.local_chat.Skip(1).ToList();
            }
            string all_messages = Converter.Encode_XML(plr_object.SelectedRoom.local_chat, typeof(List<string>));
            
            //tell everyone involved the new chat list
            foreach (Player_Connected curPlr in plr_object.SelectedRoom.room_players)
            {
                StartCoroutine(curPlr.Send_message("Room Messages", all_messages, null, null));
            }
        }

        //Game
        else if (message.reason == "Unit Adjustment" && plr_object.CurrentGame != null) // they want to make a game change
        {
            //deserilizing what they want
            XmlSerializer serilize_object1 = new XmlSerializer(typeof(List<string>));
            StringReader open_string = new StringReader(message.extra_info);

            List<string> selected_units = (List<string>)serilize_object1.Deserialize(open_string);

            object bonus = null; // bonus will be filled in if they are sending a Vector3 over

            if (message.main_message == "Move" || message.main_message == "Build")
            {
                //deserilize the list<float> as a vector3
                XmlSerializer serilize_object2 = new XmlSerializer(typeof(List<float>));
                StringReader open_string2 = new StringReader(message.extra_info2);

                List<float> params_needed = (List<float>)serilize_object2.Deserialize(open_string2);
                bonus = new Vector3(params_needed[0], params_needed[1], params_needed[2]); // change according to e2

            }
            //tell the game what they want to do
            StartCoroutine(plr_object.CurrentGame.Order_given(plr_object,  message.main_message, selected_units, bonus));
        }
        else if (message.reason == "Game Chat") // they want to chat in game
        {
            if (message.extra_info == "Private") // private chatting has a diffrent format that the game expects
            {
                StartCoroutine(plr_object.CurrentGame.New_Chat(message.main_message, "Private", plr_object, message.extra_info2));
            }
            else // tell the game to add message to the channel
            {
                StartCoroutine(plr_object.CurrentGame.New_Chat(string.Format("{0}: {1}", plr_object.Username, message.main_message),
                message.extra_info, plr_object, null));
            }
        }
        else if (message.reason == "Camera")
        { // they moved their camera in game, update the game
            if (plr_object.CurrentGame != null)
            {
                //deserilize the position
                XmlSerializer serilize_object2 = new XmlSerializer(typeof(List<float>));
                StringReader open_string2 = new StringReader(message.main_message);

                List<float> params_needed = (List<float>)serilize_object2.Deserialize(open_string2);
                plr_object.CurrentGame.Move_cam(plr_object, params_needed); // tlel the game
            }
            
        }
    }


    // ================================ Create the debug UI ================================================
    void OnGUI() // runs everytime Unity makes the UI
    {
        GUI.Label(new Rect(2, 50, 150, 100), string.Format("Players online: {0}", plr_count.ToString()));
    }

    private void OnClientAdd(NetworkMessage connection)
    { // client has been added
        //Set the connection to expect more
        
        NetworkConnection plr = connection.conn;
        
        bool channel_set = plr.SetChannelOption(channel_ID, ChannelOption.MaxPendingBuffers, 64);
        
        
        if (!channel_set)
        {
            Debug.Log("The 64 bit instead of 16 isn't working?");
        }
        players_DB[plr] = new Player_Connected // make them a Player_Connected class and store them in the server
        {
            ClientConnect = plr
        };
        plr_count++;
        //Update them with the server messages!
        StartCoroutine(players_DB[plr].Send_message("World Messages", Converter.Encode_XML(GlobalMessages, typeof(List<string>)), null, null));
    }

    //Detect when a client disconnects from the Server
    private void OnClientRemove(NetworkMessage connection)
    {
        plr_count--;
        NetworkConnection plr = connection.conn;
        Player_Connected plr_obj = players_DB[plr];

        //remove them from any rooms
        if (plr_obj.SelectedRoom != null)
        {
            StartCoroutine(plr_obj.SelectedRoom.Remove_plr(plr_obj));
        }
        // tell everyone if they had a username
        if (plr_obj.Username != null)
        {
            Add_Global(string.Format(" {0} has disconnected", plr_obj.Username));
        }
        // remove them from game if they are playing
        if (plr_obj.CurrentGame != null)
        {
            StartCoroutine(plr_obj.CurrentGame.Remove_Plr(plr_obj, 0));
        }
        players_DB.Remove(plr); // remove them from our memory
    }
}
