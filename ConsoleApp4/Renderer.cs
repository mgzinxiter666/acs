using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ClickableTransparentOverlay;
using Swed64;
using ImGuiNET;
using System.Runtime.InteropServices;
using Vortice.Direct3D11;
using System.Diagnostics;

namespace ExternoTeste
{
    public class Renderer : Overlay
    {
        [DllImport("user32.dll")]
        public static extern uint SetWindowDisplayAffinity(IntPtr hwnd, uint dwAffinity);
        const uint WDA_EXCLUDEFROMCAPTURE = 0x00000011;

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        Swed swed = new Swed("cs2");

        // Render Variaveis
        public Vector2 screenSize = new Vector2(1280, 720);

        // Entidades Copy (Verificar metodo depois)
        private ConcurrentQueue<Entity> entities = new ConcurrentQueue<Entity>();
        private Entity localPlayer = new Entity();
        private readonly object entityLock = new Object();

        // Gui Elements

        // Wallhack
        private int wallHackStyle = 0;
        private bool enableBox = true;
        public bool enableGlow = false;
        private bool ignoreTeamESP = true;
        private bool enableSkeleton = true;
        private bool enableHealthBar = false;
        private bool enableLines = false;
        private bool enableInfos = false;
        private float boneThickkness = 1;
        public bool IgnoreWalls = true;
        private bool enableTigger = false;
        private bool WallColors = true;

        // Aimbot Key Config
        private bool isSettingAimbotKey = false;
        private int aimbotKey = 0x051; // Mouse 4 como padrão
        private string aimbotKeyName = "Mouse 4";

        // Aimbot
        private bool enableAimbot = true;
        private float aimSmooth = 1.0f;
        private bool aimbotClosestToCrosshair = true;
        private bool wallAimbotIgnore = true;
        private float aimbotFov = 30;
        private bool silentAim = true;
        private float silentAimFov = 30f;
        private float silentAimSmooth = 3f;

        // config
        public bool enableBhop = true;
        private bool streamMode = false;

        // Imgui
        private bool InGuiMenu = true;
        private Vector4 enemyColor = new Vector4(255, 255, 255, 255);
        private Vector4 teamColor = new Vector4(0, 1, 0, 1);
        private Vector4 healthBarColor = new Vector4(255, 255, 255, 255);
        private Vector4 distanceColor = new Vector4(255, 255, 255, 255);
        private Vector4 fovColor = new Vector4(255, 255, 255, 255);
        private Vector4 boxColor_Wall = new Vector4(255, 255, 255, 255);
        private Vector4 skeletonColor = new Vector4(255, 255, 255, 255);
        private Vector4 linesColor = new Vector4(255, 255, 255, 255);

        static readonly string[] Ranks = {
            "Unranked",
            "Silver1",
            "Silver2",
            "Silver3",
            "Silver4",
            "Silver Elite",
            "Silver Elite Master",
            "GOLD NOVA 1",
            "GOLD NOVA 2",
            "GOLD NOVA 3",
            "GOLD NOVA Master",
            "MG1",
            "MG2",
            "MGE",
            "DMG",
            "LE",
            "LEM",
            "SMFC",
            "GE"
        };

        // Draw List
        ImDrawListPtr drawList;

        protected override void Render()
        {
            // Atualiza o Stream Mode
            if (streamMode)
            {
                // Obtém o handle da janela atual
                IntPtr hwnd = Process.GetCurrentProcess().MainWindowHandle;
                if (hwnd != IntPtr.Zero)
                {
                    SetWindowDisplayAffinity(hwnd, WDA_EXCLUDEFROMCAPTURE);
                }
            }
            else
            {
                IntPtr hwnd = Process.GetCurrentProcess().MainWindowHandle;
                if (hwnd != IntPtr.Zero)
                {
                    SetWindowDisplayAffinity(hwnd, 0);
                }
            }

            if (InGuiMenu)
            {
                var style = ImGui.GetStyle();

                style.WindowRounding = 7f;
                style.WindowBorderSize = 1f;
                style.WindowPadding = new Vector2(0, 0);
                style.ScrollbarSize = 3f;
                style.ScrollbarRounding = 0f;
                style.PopupRounding = 5f;

                style.Colors[(int)ImGuiCol.Separator] = new Vector4(0, 0, 0, 0);
                style.Colors[(int)ImGuiCol.SeparatorActive] = new Vector4(0, 0, 0, 0);
                style.Colors[(int)ImGuiCol.SeparatorHovered] = new Vector4(0, 0, 0, 0);
                style.Colors[(int)ImGuiCol.ResizeGrip] = new Vector4(0, 0, 0, 0);
                style.Colors[(int)ImGuiCol.ResizeGripActive] = new Vector4(0, 0, 0, 0);
                style.Colors[(int)ImGuiCol.ResizeGripHovered] = new Vector4(0, 0, 0, 0);
                style.Colors[(int)ImGuiCol.PopupBg] = new Vector4(14f / 255f, 14f / 255f, 14f / 255f, 1f);

                style.Colors[(int)ImGuiCol.ScrollbarBg] = new Vector4(0, 0, 0, 0);
                style.Colors[(int)ImGuiCol.ScrollbarGrab] = new Vector4(232f / 255f, 63f / 255f, 212f / 255f, 1f);
                style.Colors[(int)ImGuiCol.ScrollbarGrabActive] = new Vector4(232f / 255f, 63f / 255f, 212f / 255f, 1f);
                style.Colors[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(232f / 255f, 63f / 255f, 212f / 255f, 1f);

                style.Colors[(int)ImGuiCol.WindowBg] = new Vector4(14f / 255f, 14f / 255f, 14f / 255f, 1f);
                style.Colors[(int)ImGuiCol.Border] = new Vector4(24f / 255f, 23f / 255f, 25f / 255f, 1f);


                ImGui.SetNextWindowSize(new Vector2(800, 600), ImGuiCond.FirstUseEver);
                ImGui.Begin("mgzinclient");

                if (ImGui.BeginTabBar("Tabs"))
                {
                    if (ImGui.BeginTabItem("Menu"))
                    {
                        if (ImGui.CollapsingHeader("Aimbot Config"))
                        {
                            ImGui.Checkbox("Aimbot", ref enableAimbot);
                            ImGui.Checkbox("mira", ref aimbotClosestToCrosshair);
                            ImGui.SliderFloat("Suavizacao do Aim", ref aimSmooth, 1.0f, 10.0f);
                            ImGui.Checkbox("TriggerBot", ref enableTigger);
                            ImGui.Checkbox("Ignore Walls [Aim]", ref wallAimbotIgnore);
                            ImGui.SliderFloat("FOV", ref aimbotFov, 10f, 300f);

                            // Botão para configurar a tecla do aimbot
                            if (ImGui.Button($"Aimbot Key: {aimbotKeyName}"))
                            {
                                isSettingAimbotKey = true;
                            }

                            // Se estiver configurando a tecla, exiba "Press Key..."
                            if (isSettingAimbotKey)
                            {
                                ImGui.SameLine();
                                ImGui.Text("Press Key...");

                                // Verifique todas as teclas do teclado e mouse
                                for (int i = 0; i < 256; i++)
                                {
                                    if (GetAsyncKeyState(i) != 0)
                                    {
                                        aimbotKey = i;
                                        aimbotKeyName = GetKeyName(i);
                                        isSettingAimbotKey = false;
                                        break;
                                    }
                                }
                            }
                        }

                        if (ImGui.CollapsingHeader("Wallhack"))
                        {
                            ImGui.Checkbox("Box", ref enableBox);
                            ImGui.Checkbox("Glow", ref enableGlow);
                            ImGui.Checkbox("Skeleton", ref enableSkeleton);
                            ImGui.Checkbox("Lines", ref enableLines);
                            ImGui.Checkbox("HealthBar", ref enableHealthBar);
                            ImGui.Checkbox("Infos", ref enableInfos);

                            if (ImGui.CollapsingHeader("Config [Wallhack]"))
                            {
                                ImGui.Text("WallHack Style");
                                if (ImGui.Button("-"))
                                {
                                    if (wallHackStyle >= 1)
                                        wallHackStyle--;
                                }
                                ImGui.SameLine();
                                ImGui.Text($"{wallHackStyle}");
                                ImGui.SameLine();
                                if (ImGui.Button("+"))
                                {
                                    if (wallHackStyle < 1)
                                        wallHackStyle++;
                                }

                                ImGui.Checkbox("Ignore Team", ref ignoreTeamESP);
                                ImGui.Checkbox("Ignore Walls", ref IgnoreWalls);
                                ImGui.Checkbox("Wall Colors", ref WallColors);
                            }
                        }
                        ImGui.EndTabItem();
                    }

                    if (ImGui.BeginTabItem("Colors"))
                    {
                        if (ImGui.CollapsingHeader("team color"))
                        {
                            ImGui.Text("Team Color");
                            ImGui.Dummy(new Vector2(0.0f, 2.0f));
                            ImGui.ColorPicker4("##teamcolor", ref teamColor);
                            ImGui.Dummy(new Vector2(0.0f, 20.0f));
                        }
                        if (ImGui.CollapsingHeader("enemy color"))
                        {
                            ImGui.Text("Enemy Color");
                            ImGui.Dummy(new Vector2(0.0f, 2.0f));
                            ImGui.ColorPicker4("##enemycolor", ref enemyColor);
                            ImGui.Dummy(new Vector2(0.0f, 20.0f));
                        }

                        if (ImGui.CollapsingHeader("box color"))
                        {
                            ImGui.Text("Enemy Box Color In Wall");
                            ImGui.Dummy(new Vector2(0.0f, 2.0f));
                            ImGui.ColorPicker4("##boxcolor_wall", ref boxColor_Wall);
                            ImGui.Dummy(new Vector2(0.0f, 20.0f));
                        }

                        if (ImGui.CollapsingHeader("skeleton color"))
                        {
                            ImGui.Text("Skeleton Color");
                            ImGui.Dummy(new Vector2(0.0f, 2.0f));
                            ImGui.ColorPicker4("##skeleton", ref skeletonColor);
                            ImGui.Dummy(new Vector2(0.0f, 20.0f));
                        }

                        if (ImGui.CollapsingHeader("Health color"))
                        {
                            ImGui.Text("Health Color");
                            ImGui.Dummy(new Vector2(0.0f, 2.0f));
                            ImGui.ColorPicker4("##healthColor", ref healthBarColor);
                            ImGui.Dummy(new Vector2(0.0f, 20.0f));
                        }

                        if (ImGui.CollapsingHeader("Distance color"))
                        {
                            ImGui.Text("Distance Color");
                            ImGui.Dummy(new Vector2(0.0f, 2.0f));
                            ImGui.ColorPicker4("##distanceColor", ref distanceColor);
                            ImGui.Dummy(new Vector2(0.0f, 20.0f));
                        }

                        if (ImGui.CollapsingHeader("Fov color"))
                        {
                            ImGui.Text("FOV Color");
                            ImGui.Dummy(new Vector2(0.0f, 2.0f));
                            ImGui.ColorPicker4("##fovcolor", ref fovColor);
                            ImGui.Dummy(new Vector2(0.0f, 20.0f));
                        }

                        if (ImGui.CollapsingHeader("Line color"))
                        {
                            ImGui.Text("Lines");
                            ImGui.Dummy(new Vector2(0.0f, 2.0f));
                            ImGui.ColorPicker4("##linesColor", ref linesColor);
                            ImGui.Dummy(new Vector2(0.0f, 20.0f));
                        }
                        ImGui.EndTabItem();
                    }

                    if (ImGui.BeginTabItem("config  "))
                    {
                        ImGui.Checkbox("Stream Mode", ref streamMode);
                        if (streamMode)
                        {
                            ImGui.TextColored(new Vector4(1, 0, 0, 1), "ATENÇÃO: Stream Mode ativado - não aparecerá em gravações");
                        }
                        ImGui.EndTabItem();
                    }
                }
            }

            DrawOverlay(screenSize);
            drawList = ImGui.GetWindowDrawList();

            if (enableAimbot)
                DrawFov();

            if (GetAsyncKeyState(Offsets.imguiKey) < 0)
            {
                if (InGuiMenu) InGuiMenu = false;
                else InGuiMenu = true;
                Thread.Sleep(20);
            }

            Entity bestTarget = null;
            float bestMetric = float.MaxValue;

            foreach (var entity in entities)
            {
                if (ignoreTeamESP && entity.team == localPlayer.team)
                    continue;

                if (!EntityOnScreen(entity))
                    continue;

                if (!wallAimbotIgnore && !entity.spotted)
                    continue;

                if (aimbotClosestToCrosshair)
                {
                    float distanceToCrosshair = Vector2.Distance(entity.head2d, new Vector2(screenSize.X / 2, screenSize.Y / 2));

                    if (distanceToCrosshair < aimbotFov && distanceToCrosshair < bestMetric)
                    {
                        bestMetric = distanceToCrosshair;
                        bestTarget = entity;
                    }
                }
                else
                {
                    float distanceToPlayer = Vector3.Distance(localPlayer.position, entity.position);

                    if (distanceToPlayer < bestMetric)
                    {
                        bestMetric = distanceToPlayer;
                        bestTarget = entity;
                    }
                }

                DrawCheats(entity);

                if (enableTigger && GetAsyncKeyState(aimbotKey) < 0)
                {
                    int entIndex = swed.ReadInt(Offsets.PlayerPawn, Offsets.m_iIDEntIndex);
                    Console.Clear();
                    Console.WriteLine(entIndex);
                    if (entIndex != -1)
                    {
                        swed.WriteInt(Offsets.forceAttack, 65537);
                        Thread.Sleep(1);
                        swed.WriteInt(Offsets.forceAttack, 256);
                    }
                    Thread.Sleep(1);
                }
            }

            if (enableAimbot && bestTarget != null && GetAsyncKeyState(aimbotKey) < 0)
            {
                Vector3 playerView = localPlayer.position + localPlayer.viewOffset;
                Vector2 targetAngles2D = Calculate.CalculateAngles(playerView, bestTarget.head);
                Vector3 targetAngles = new Vector3(targetAngles2D.Y, targetAngles2D.X, 0.0f);
                Vector3 currentAngles = swed.ReadVec(Offsets.client, Offsets.dwViewAngles);
                Vector3 smoothedAngles = SmoothAngle(currentAngles, targetAngles, aimSmooth);
                swed.WriteVec(Offsets.client, Offsets.dwViewAngles, smoothedAngles);
            }
        }

        private string GetKeyName(int keyCode)
        {
            switch (keyCode)
            {
                case 0x01: return "Mouse 1";
                case 0x02: return "Mouse 2";
                case 0x04: return "Mouse 3";
                case 0x05: return "Mouse 4";
                case 0x06: return "Mouse 5";
                case 0x08: return "Backspace";
                case 0x09: return "Tab";
                case 0x0D: return "Enter";
                case 0x10: return "Shift";
                case 0x11: return "Ctrl";
                case 0x12: return "Alt";
                case 0x14: return "Caps Lock";
                case 0x1B: return "Esc";
                case 0x20: return "Space";
                case 0x25: return "Left Arrow";
                case 0x26: return "Up Arrow";
                case 0x27: return "Right Arrow";
                case 0x28: return "Down Arrow";
                default:
                    if (keyCode >= 0x30 && keyCode <= 0x5A)
                    {
                        return ((char)keyCode).ToString();
                    }
                    return $"Key 0x{keyCode:X}";
            }
        }

        void AimAt(Vector3 angles)
        {
            swed.WriteFloat(Offsets.client, Offsets.dwViewAngles, angles.Y);
            swed.WriteFloat(Offsets.client, Offsets.dwViewAngles + 0x4, angles.X);
        }

        public float CalculateMagnitude(Vector3 v1, Vector3 v2)
        {
            return (float)Math.Sqrt(Math.Pow(v2.X - v1.X, 2) + Math.Pow(v2.Y - v1.Y, 2) + Math.Pow(v2.Z - v1.Z, 2));
        }

        bool EntityOnScreen(Entity entity)
        {
            if (entity.position2D.X > 0 && entity.position2D.X < screenSize.X && entity.position2D.Y > 0 && entity.position2D.Y < screenSize.Y)
            {
                return true;
            }
            return false;
        }

        public void DrawFov()
        {
            drawList.AddCircle(new Vector2(screenSize.X / 2, screenSize.Y / 2), aimbotFov, ImGui.ColorConvertFloat4ToU32(fovColor));
        }

        public List<Vector3> ReadBones(IntPtr boneAddress)
        {
            byte[] boneBytes = swed.ReadBytes(boneAddress, 27 * 32 + 16);
            List<Vector3> bones = new List<Vector3>();
            foreach (var boneId in Enum.GetValues(typeof(BonesIds)))
            {
                float x = BitConverter.ToSingle(boneBytes, (int)boneId * 32 + 0);
                float y = BitConverter.ToSingle(boneBytes, (int)boneId * 32 + 4);
                float z = BitConverter.ToSingle(boneBytes, (int)boneId * 32 + 8);
                Vector3 currentBone = new Vector3(x, y, z);
                bones.Add(currentBone);
            }
            return bones;
        }

        public List<Vector2> ReadBones2d(List<Vector3> bones, ViewMatrix viewMatrix, Vector2 screenSize)
        {
            List<Vector2> bones2d = new List<Vector2>();
            foreach (Vector3 bone in bones)
            {
                Vector2 bone2d = Calculate.WorldToScreenMatrix(viewMatrix, bone, (int)screenSize.X, (int)screenSize.Y);
                bones2d.Add(bone2d);
            }
            return bones2d;
        }

        public ViewMatrix readMatrix(IntPtr matrixAddress)
        {
            var viewMatrix = new ViewMatrix();
            var matrix = swed.ReadMatrix(matrixAddress);

            viewMatrix.m11 = matrix[0];
            viewMatrix.m12 = matrix[1];
            viewMatrix.m13 = matrix[2];
            viewMatrix.m14 = matrix[3];

            viewMatrix.m21 = matrix[4];
            viewMatrix.m22 = matrix[5];
            viewMatrix.m23 = matrix[6];
            viewMatrix.m24 = matrix[7];

            viewMatrix.m31 = matrix[8];
            viewMatrix.m32 = matrix[9];
            viewMatrix.m33 = matrix[10];
            viewMatrix.m34 = matrix[11];

            viewMatrix.m41 = matrix[12];
            viewMatrix.m42 = matrix[13];
            viewMatrix.m43 = matrix[14];
            viewMatrix.m44 = matrix[15];
            return viewMatrix;
        }

        private void DrawCheats(Entity entity)
        {
            if (!IgnoreWalls && !entity.spotted)
                return;

            float entityHeight = entity.position2D.Y - entity.viewPosition2D.Y;
            float cornerLength = 10.0f;
            Vector2 rectTop = new Vector2(entity.viewPosition2D.X - entityHeight / 3, entity.viewPosition2D.Y);
            Vector2 rectBottom = new Vector2(entity.position2D.X + entityHeight / 3, entity.position2D.Y);
            Vector4 boxColor = localPlayer.team == entity.team ? teamColor : enemyColor;

            if (enableBox)
            {
                uint wallColorF;
                if (WallColors) { wallColorF = entity.spotted ? (ImGui.ColorConvertFloat4ToU32(boxColor)) : (ImGui.ColorConvertFloat4ToU32(boxColor_Wall)); }
                else { wallColorF = ImGui.ColorConvertFloat4ToU32(boxColor); }

                switch (wallHackStyle)
                {
                    case 0:
                        drawList.AddRect(rectTop, rectBottom, wallColorF);
                        break;
                    case 1:
                        drawList.AddLine(rectTop, new Vector2(rectTop.X, rectTop.Y + cornerLength), wallColorF);
                        drawList.AddLine(rectTop, new Vector2(rectTop.X + cornerLength, rectTop.Y), wallColorF);

                        Vector2 topRight = new Vector2(rectBottom.X, rectTop.Y);
                        drawList.AddLine(topRight, new Vector2(topRight.X, topRight.Y + cornerLength), wallColorF);
                        drawList.AddLine(topRight, new Vector2(topRight.X - cornerLength, topRight.Y), wallColorF);

                        Vector2 bottomLeft = new Vector2(rectTop.X, rectBottom.Y);
                        drawList.AddLine(bottomLeft, new Vector2(bottomLeft.X, bottomLeft.Y - cornerLength), wallColorF);
                        drawList.AddLine(bottomLeft, new Vector2(bottomLeft.X + cornerLength, bottomLeft.Y), wallColorF);

                        drawList.AddLine(rectBottom, new Vector2(rectBottom.X, rectBottom.Y - cornerLength), wallColorF);
                        drawList.AddLine(rectBottom, new Vector2(rectBottom.X - cornerLength, rectBottom.Y), wallColorF);
                        break;
                }
            }

            if (enableHealthBar)
            {
                float barWidth = 4.0f;
                float gap = 3.0f;
                float healthPercent = Math.Clamp((float)entity.health / 100f, 0f, 1f);

                Vector2 barTopLeft = new Vector2(rectTop.X - gap - barWidth, rectTop.Y);
                Vector2 barBottomRight = new Vector2(rectTop.X - gap, rectBottom.Y);

                Vector4 healthColor;
                if (healthPercent > 0.6f)
                {
                    float intensity = (healthPercent - 0.6f) / 0.4f;
                    healthColor = new Vector4(0f, intensity, 0f, 1f);
                }
                else if (healthPercent > 0.3f)
                {
                    float intensity = (healthPercent - 0.3f) / 0.3f;
                    healthColor = new Vector4(intensity, intensity, 0f, 1f);
                }
                else
                {
                    float intensity = healthPercent / 0.3f;
                    healthColor = new Vector4(1f, intensity, 0f, 1f);
                }

                drawList.AddRectFilled(barTopLeft, barBottomRight, ImGui.ColorConvertFloat4ToU32(new Vector4(0.1f, 0.1f, 0.1f, 0.8f)));
                float healthHeight = (barBottomRight.Y - barTopLeft.Y) * healthPercent;
                Vector2 healthBarTop = new Vector2(barTopLeft.X, barBottomRight.Y - healthHeight);
                drawList.AddRectFilled(healthBarTop, barBottomRight, ImGui.ColorConvertFloat4ToU32(healthColor));
                drawList.AddRect(barTopLeft, barBottomRight, ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f, 1f)), 0f, ImDrawFlags.None, 1f);

                string healthText = $"{entity.health}";
                Vector2 textSize = ImGui.CalcTextSize(healthText);
                Vector2 textPos = new Vector2(barTopLeft.X - textSize.X - 2f, barBottomRight.Y - healthHeight - textSize.Y / 2);
                drawList.AddText(textPos, ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 1f)), healthText);
            }

            if (enableInfos)
            {
                float infoBoxHeight = 60.0f;
                float infoBoxWidth = rectBottom.X - rectTop.X + 10.0f;
                Vector2 infoBoxTopLeft = new Vector2(rectTop.X - 5.0f, rectBottom.Y + 5.0f);
                Vector2 infoBoxBottomRight = new Vector2(infoBoxTopLeft.X + infoBoxWidth, infoBoxTopLeft.Y + infoBoxHeight);
                Vector4 infoBoxColor = new Vector4(0, 0, 0, 0.5f);
                drawList.AddRectFilled(infoBoxTopLeft, infoBoxBottomRight, ImGui.ColorConvertFloat4ToU32(infoBoxColor));

                string distanceText = $"Dist.: {entity.distance:F}";
                string rankText = $"Rank: {Ranks[entity.rank]}";

                drawList.AddText(new Vector2(infoBoxTopLeft.X + 5, infoBoxTopLeft.Y + 5), ImGui.ColorConvertFloat4ToU32(new Vector4(1, 1, 1, 1)), distanceText);
                drawList.AddText(new Vector2(infoBoxTopLeft.X + 5, infoBoxTopLeft.Y + 25), ImGui.ColorConvertFloat4ToU32(new Vector4(1, 1, 1, 1)), rankText);
            }

            if (enableSkeleton)
            {
                drawList.AddLine(entity.bones2d[1], entity.bones2d[2], ImGui.ColorConvertFloat4ToU32(skeletonColor), boneThickkness);
                drawList.AddLine(entity.bones2d[1], entity.bones2d[3], ImGui.ColorConvertFloat4ToU32(skeletonColor), boneThickkness);
                drawList.AddLine(entity.bones2d[1], entity.bones2d[6], ImGui.ColorConvertFloat4ToU32(skeletonColor), boneThickkness);
                drawList.AddLine(entity.bones2d[3], entity.bones2d[4], ImGui.ColorConvertFloat4ToU32(skeletonColor), boneThickkness);
                drawList.AddLine(entity.bones2d[6], entity.bones2d[7], ImGui.ColorConvertFloat4ToU32(skeletonColor), boneThickkness);
                drawList.AddLine(entity.bones2d[4], entity.bones2d[5], ImGui.ColorConvertFloat4ToU32(skeletonColor), boneThickkness);
                drawList.AddLine(entity.bones2d[7], entity.bones2d[8], ImGui.ColorConvertFloat4ToU32(skeletonColor), boneThickkness);
                drawList.AddLine(entity.bones2d[1], entity.bones2d[0], ImGui.ColorConvertFloat4ToU32(skeletonColor), boneThickkness);
                drawList.AddLine(entity.bones2d[0], entity.bones2d[9], ImGui.ColorConvertFloat4ToU32(skeletonColor), boneThickkness);
                drawList.AddLine(entity.bones2d[0], entity.bones2d[11], ImGui.ColorConvertFloat4ToU32(skeletonColor), boneThickkness);
                drawList.AddLine(entity.bones2d[9], entity.bones2d[10], ImGui.ColorConvertFloat4ToU32(skeletonColor), boneThickkness);
                drawList.AddLine(entity.bones2d[11], entity.bones2d[12], ImGui.ColorConvertFloat4ToU32(skeletonColor), boneThickkness);
            }

            if (enableLines)
            {
                drawList.AddLine(new Vector2(screenSize.X / 2, screenSize.Y), entity.position2D, ImGui.ColorConvertFloat4ToU32(linesColor));
            }
        }

        public void UpdateEntities(IEnumerable<Entity> newEntities)
        {
            entities = new ConcurrentQueue<Entity>(newEntities);
        }

        public void UpdateLocalPlayer(Entity newEntity)
        {
            lock (entityLock)
            {
                localPlayer = newEntity;
            }
        }

        Vector3 SmoothAngle(Vector3 current, Vector3 target, float smooth)
        {
            Vector3 delta = ClampAngles(target - current);

            if (smooth <= 1f)
                return ClampAngles(current + delta);

            return ClampAngles(current + delta / smooth);
        }

        Vector3 ClampAngles(Vector3 angles)
        {
            if (angles.X > 89.0f) angles.X = 89.0f;
            if (angles.X < -89.0f) angles.X = -89.0f;

            while (angles.Y > 180.0f) angles.Y -= 360.0f;
            while (angles.Y < -180.0f) angles.Y += 360.0f;

            angles.Z = 0.0f;
            return angles;
        }

        void DrawOverlay(Vector2 screenSize)
        {
            ImGui.SetNextWindowSize(screenSize);
            ImGui.SetNextWindowPos(new Vector2(0, 0));
            ImGui.Begin("overlay", ImGuiWindowFlags.NoDecoration
                | ImGuiWindowFlags.NoBackground
                | ImGuiWindowFlags.NoBringToFrontOnFocus
                | ImGuiWindowFlags.NoMove
                | ImGuiWindowFlags.NoInputs
                | ImGuiWindowFlags.NoCollapse
                | ImGuiWindowFlags.NoScrollbar
                | ImGuiWindowFlags.NoScrollWithMouse);
        }

        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(int vKey);
    }
}