using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogComponent.Test;

internal static class Helpers
{
    public static DateTime RandomizeTime(this DateTime time)
    {
        Random random = new Random();

        int randomHours = random.Next(0, 24);
        int randomMinutes = random.Next(0, 60);
        int randomSeconds = random.Next(0, 60);

        DateTime randomizedDateTime = new(time.Year, time.Month, time.Day,
            randomHours, randomMinutes, randomSeconds);

        return randomizedDateTime;
    }

    public static int CountFilesInFolder(string folderPath)
    {
        return Directory.Exists(folderPath) ? Directory.GetFiles(folderPath).Length : -1;
    }
}
