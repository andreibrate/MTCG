﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace MTCG.Models
{
    public class User
    {
        // standard constructor
        public User()
        { 
            // standard properties to avoid null reference
            Username = string.Empty;
            Password = string.Empty;
            Coins = 20;                // initial coins start amount
            Stack = new Stack();       // all cards of user
            Deck = new Deck();         // best 4 cards of user used in battle
            Stats = new Stats();       
            Bio = string.Empty;
            Image = string.Empty;
        }

        // constructor for registration
        public User(string username, string password)
        {
            Username = username;
            Password = password;
            Coins = 20;
            Stack = new Stack();
            Deck = new Deck();
            Stats = new Stats();
            Bio = string.Empty;
            Image = string.Empty;
        }

        public Guid Id { get; set; }
        public string Username { get; set; } // unique
        public string Password { get; set; } // hashed
        public int Coins { get; set; }
        public Stack Stack { get; set; } = new Stack();   // all cards a user owns
        public Deck Deck { get; set; } = new Deck();     // best 4 cards
        public Stats Stats { get; set; } = new Stats();
        public string? Bio { get; set; }   // can be missing (null)
        public string? Image { get; set; } // can be missing (null)
    }
}
