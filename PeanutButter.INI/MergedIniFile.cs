namespace PeanutButter.INI
{
    internal class MergedIniFile
    {
        internal MergedIniFile(
            IINIFile iniFile,
            MergeStrategies mergeStrategy)
        {
            IniFile = iniFile;
            MergeStrategy = mergeStrategy;
        }

        public IINIFile IniFile { get; }
        public MergeStrategies MergeStrategy { get; }
    }
}