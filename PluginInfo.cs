namespace SeralythTemp
{
    public class PluginInfo
    {
#if LEGAL
        public const bool LegalBuild = true;
#else
        public const bool LegalBuild = false;
#endif
        public const string GUID = LegalBuild ? "org.1x1x1x1VR12.gorillatag.menutemplate.legal" : "org.1x1x1x1VR12.gorillatag.menutemplate";
        public const string Name = LegalBuild ? "Seralyth Temp Legal" : "Seralyth Temp";
        public const string Description = "Created by @1x1x1x1VR12 with love <3";
        public const string Version = "1.0.0";
    }
}
