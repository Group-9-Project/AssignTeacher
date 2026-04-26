using System;
using System.Configuration;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace SchoolTimetable.Services
{
    public class JitsiService
    {
        private static string Domain =>
            ConfigurationManager.AppSettings["Jitsi:Domain"] ?? "meet.jit.si";

        public static string GenerateRoomId(string classTitle, int teacherId)
        {
            var raw = string.Format("{0}-{1}-{2:N}", classTitle, teacherId, Guid.NewGuid());
            var safe = new string(raw.ToLowerInvariant()
                .Select(c => char.IsLetterOrDigit(c) ? c : '-').ToArray());

            while (safe.Contains("--"))
                safe = safe.Replace("--", "-");

            safe = safe.Trim('-');
            var trimmed = safe.Length > 40 ? safe.Substring(0, 40) : safe;
            return "dgss-class-" + trimmed;
        }
        
        public static string GetMeetingUrl(string roomId)
        {
            return string.Format("https://{0}/{1}", Domain, Uri.EscapeDataString(roomId));
        }

        public static string GetJitsiConfig(string roomId, string displayName, bool isModerator)
        {
            var toolbarTeacher = new[] { "microphone", "camera", "desktop", "chat",
                "raisehand", "participants-pane", "tileview", "hangup",
                "mute-everyone", "security" };

            var toolbarStudent = new[] { "microphone", "camera", "chat",
                "raisehand", "tileview", "hangup" };

            var config = new
            {
                domain = Domain,
                roomName = roomId,
                displayName = displayName,
                isModerator = isModerator,
                userInfo = new { displayName = displayName },
                configOverwrite = new
                {
                    startWithAudioMuted = !isModerator,
                    startWithVideoMuted = !isModerator,
                    enableWelcomePage = false,
                    prejoinPageEnabled = false,
                    disableDeepLinking = true,
                    subject = "Online Class"
                },
                interfaceConfigOverwrite = new
                {
                    SHOW_JITSI_WATERMARK = false,
                    SHOW_WATERMARK_FOR_GUESTS = false,
                    TOOLBAR_BUTTONS = isModerator ? toolbarTeacher : toolbarStudent
                }
            };

            return JsonConvert.SerializeObject(config);
        }
    }
}
