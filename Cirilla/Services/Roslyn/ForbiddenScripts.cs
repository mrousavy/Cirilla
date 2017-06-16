namespace Cirilla.Services.Roslyn {
    public class ForbiddenScripts {
        public static bool IsForbiddenScript(string code) {
            if (code.Contains("Process") ||
                code.Contains("Assembly") ||
                code.Contains("AppDomain") ||
                code.Contains(".Kill()") ||
                code.Contains("File")) {
                return true;
            }
            return false;
        }
    }
}
