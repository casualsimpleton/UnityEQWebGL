﻿//Gender Enum - Might as well use the enum
//Races Enum - To avoid the need for string matching and since the server will be communicating via numerics, let's lay these out as enums
//CasualSimpleton

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using EQBrowser;

namespace EQBrowser
{
    public enum Gender : byte
    {
        Male = 0,
        Female = 1,
        It = 2, //Genderless - appears as "it" in emotes

        AssaultHelicopter = 255
    }

    /// <summary>
    /// These are the races. Only going to bother doing some of the basics for now. They need to match what the server is sending. 
    //http://eoc.akkadius.com/EOC2/min.php?Mod=RaceViewer&RaceView=1
    /// </summary>
    public enum Race : int
    {
        Default = 0,
        Human = 1,
        Barbarian = 2,
        Erudite = 3,
        WoodElf = 4,
        HighElf = 5,
        DarkElf = 6,
        HalfElf = 7,
        Dwarf = 8,
        Troll = 9,
        Ogre = 10,
        Halfling = 11,
        Gnome = 12,
        Aviak = 13,
        Werewolf = 14,
        Brownie = 15,
        Centaur = 16,
        Golen = 17,
        GiantCyclops = 17,
        Trakanon = 19,
        VenrilSathir = 20,
        EvilEye = 21,
        Beetle = 22,
        Kerran = 23,
        Fish = 24,
        Fairy = 25,
        Froglok = 26,
        FroglokGhoul = 27,
        FungusMan = 28,
        Gargoyle = 29,
        Gasbas = 30,
        GelCube = 31,
        Ghost = 32,
        Ghoul = 33,
        GiantBat = 34,
        GiantEel = 35,
        GiantRat = 36,
        GiantSnake = 37,
        GiantSpider = 38,
        Gnoll = 39,
        Goblin = 40,
        Gorilla = 41,
        Wolf = 42,
        Bear = 43,
        FreeportGuard = 44,
        DemiLich = 45,
        Imp = 46,
        Griffin = 47,
        Kobold = 48,
        LavaDragon = 49,
        Lion = 50,
        LizardMan = 51,
        Mimic = 52,
        Minotaur = 53,
        Orc = 54,
        HumanBeggar = 55,
        Pixie = 56,
        Dracnid = 57,
        SolusekRo = 58,
        BloodgillGoblin = 59,
        Skeleton = 60,
        Shark = 61,
        Tunare = 62,
        Tiger = 63,
        Treant = 64,
        Vampire = 65,
        StatueOfRallosZek = 66,
        HighpassCitizen = 67,
        Tentacle = 68,
        Wisp = 69,
        Zombie = 70,
        QeynosCitizen = 71,
        Ship = 72,
        Launch = 73, //?
        Piranha = 74,
        Elemental = 75,
        Puma = 76,
        NeriakCitizen = 77,
        EruditeCitizen = 78,
        Bixie = 79,
        ReanimatedHand = 80,
        RivervaleCitizen = 81,
        Scarecrow = 82,
        Skunk = 83,
        SnakeElemental = 84, //?
        Spectre = 85,
        Sphinx = 86,
        Armadillo = 87,
        ClockworkGnome = 88,
        Drake = 89,
        HalasCitizen = 90,
        Alligator = 91,
        GrobbCitizen = 92,
        OggokCitizen = 93,
        KaladimCitizen = 94,
        CazicThule = 95,
        Cockatrice = 96,
        DaisyMan = 97, //?
        ElfVampire = 98,
        Denizen = 99,
        Dervish = 100,
        Efreeti = 101,
        FroglokTadpole = 102,
        PhinigelAutropos = 103,
        Leech = 104,
        Swordfish = 105,
        Felguard = 106,
        Mammoth = 107,
        EyeOfZomm = 108,
        Wasp = 109,
        Mermaid = 110,
        Harpie = 111,
        Fayguard = 112,
        Drixie = 113,
        GhostShip = 114,
        Clam = 115,
        SeaHorse = 116,
        DwarfGhost = 117,
        EruditeGhost = 118,
        Sabertooth = 119,
        WolfElemental = 120,
        Gorgon = 121,
        DragonSkeleton = 122,
        Innoruuk = 123,
        Unicorn = 124,
        Pegasus = 125,
        Djinn = 126,
        InvisibleMan = 127,
        Iksar = 128,
        Scorpion = 129,
        VahShir = 130,
        Sarnak = 131,
        Draglock = 132,
        Lycanthrope = 133,
        Mosquito = 134,
        Rhino = 135,
        Xalgoz = 136,
        KunarkGoblin = 137,
        Yeti = 138,
        IksarCitizen = 139,
        ForestGiant = 140,
        Boat = 141,
        MinorIllusion = 142,
        TreeIllusion = 143,
        Burynai = 144,
        Goo = 145,
        SpectralSarnak = 146,
        SpectralIksar = 147,
        KunkarkFish = 148,
        IksarScorpion = 149,
        Erollisi = 150,
        Tribunal = 151,
        Bertoxxulous = 152,
        Bristlebane = 153,
        FayDrake = 154,
        SarnakSkeleton = 155,
        Ratman = 156,
        Wyvern = 157,
        Wurm = 158,
        Devourer = 159,
        IksarGolem = 160,
        IksarSkeleton = 161,
        ManEatingPlant = 162,
        Raptor = 163,
        SarnakGolen = 164,
        WaterDragon = 165,
        IksarHand = 166,
        Succulent = 167,
        Holgresh = 168,
        Brontotherium = 169,
        SnowDervish = 170,
        DireWolf = 171,
        Manticore = 172,
        Totem = 173,
        ColdSpectre = 174,
        EnchantedArmor = 175,
        SnowBunny = 176,
        Walrus = 177,
        RockgemMan = 178,
        Unknown179 = 179,
        Unknown180 = 180,
        YakMan = 181,
        Faun = 182,
        Coldain = 183,
        VeliousDragon = 184,
        Hag = 185,
        Hippogriff = 186,
        Siren = 187,
        FrostGiant = 188,
        StormGiant = 189,
        Otterman = 190,
        WalrusMan = 191,
        ClockworkDragon = 192,
        Abhorrent = 193,
        SeaTurtle = 194,
        BlackAndWhiteDragon = 195,
        GhostDragon = 196,
        RonnieTest = 197, //?
        PrismaticDrahgon = 198
        
        //Luclin \/\/\/\/\/
    }
}