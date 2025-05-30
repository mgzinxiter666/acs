using Swed64;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternoTeste
{
    public class Offsets
    {
        public const int imguiKey = 0x2D;
        public const int aimbotKey = 0x04;
        public const int jumpKey = 0x20;

        public static IntPtr client { get; set; }

        public static Entity? localPlayer { get; set; }
        public static Swed? swed { get; set; }

        // offsets.cs
        public static int dwViewAngles = 0x1A733C0;
        public static int dwEntityList = 0x19FFE48;
        public static int dwViewMatrix = 0x1A68FD0;
        public static int dwLocalPlayerPawn = 0x18540D0;
        public static int dwForceJump = 0x1730530;
        public static int dwForceAttack = 0x1730020;
        public static int dwGlowManager = 0x1A64010;

        // client.dll.cs
        public static int m_vOldOrigin = 0x1324;
        public static int m_iTeamNum = 0x3E3;
        public static int m_lifeState = 0x348;
        public static int m_hPlayerPawn = 0x824;
        public static int m_vecViewOffset = 0xCB0;
        public static int m_iHealth = 0x344;
        public static int m_iCompetitiveRanking = 0x798;
        public static int m_pGameSceneNode = 0x328;
        public static int m_modelState = 0x170;
        public static int m_entitySpottedState = 0x23D0;
        public static int m_bSpotted = 0x8;
        public static int m_flDetectedByEnemySensorTime = 0x1440;
        public static int m_fFlags = 0x63;
        public static int m_iIDEntIndex = 0x1458;
        

        // config [my]
        public static IntPtr PlayerPawn { get; set; }
        public static IntPtr forceAttack = Offsets.client + Offsets.dwForceAttack;
    }
}
