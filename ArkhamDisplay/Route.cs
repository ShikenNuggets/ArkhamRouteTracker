using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace ArkhamDisplay;

public class Entry(string name, string type, string id, string alternateID = null, string metadata = null)
{
    public string name = name;
    public string type = type;
    public string id = id;
    public string alternateID = alternateID;
    public string metadata = metadata;

    public bool IsType(string type_)
    {
        if (string.IsNullOrWhiteSpace(type_))
        {
            return false;
        }

        return type_.Equals(type);
    }

    public static bool IsPlaceholder(Entry e) =>
        e.IsType("[PLACEHOLDER]");
}

public class Route
{
    private readonly string fileName;
    public List<Entry> entries;

    public Route(string file, List<string> data = null)
    {
        fileName = file;
        entries = [];

        if (string.IsNullOrWhiteSpace(fileName) && data == null)
        {
            throw new NullReferenceException("Invalid route file name or data!");
        }

        IEnumerable<string> allLines = [];
        if (fileName != null)
        {
            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException("Could not find route file at [" + fileName + "]", fileName);
            }

            allLines = File.ReadAllLines(fileName).Skip(1);
        }
        else if (data != null)
        {
            allLines = data.Skip(1);
        }

        entries.Capacity = allLines.Count();
        foreach (string line in allLines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue; //Ignore blank lines
            }

            string[] lineComponents = line.Split('\t');
            if (lineComponents.Length < 3)
            {
                throw new Exception("Could not load route! All entries must have at least 3 columns!");
            }

            string optionalAltID = null;
            string optionalMetaData = null;
            if (lineComponents.Length >= 4)
            {
                optionalAltID = lineComponents[3].Trim();
            }
            if (lineComponents.Length >= 5)
            {
                optionalMetaData = lineComponents[4].Trim();
            }

            entries.Add(new Entry(
                lineComponents[0].Trim(),
                lineComponents[1].Trim(),
                lineComponents[2].Trim(),
                optionalAltID,
                optionalMetaData
            ));
        }
    }

    public List<Entry> GetEntriesWithPlaceholdersMoved()
    {
        var newEntries = new List<Entry>(entries);

        var placeHolders = newEntries.FindAll(Entry.IsPlaceholder);
        foreach (var p in placeHolders)
        {
            var onesToMove = newEntries.FindAll(x => x.IsType(p.name));
            newEntries.RemoveAll(x => x.IsType(p.name));
            newEntries.InsertRange(newEntries.FindIndex(x => x == p), onesToMove);
        }

        newEntries.RemoveAll(Entry.IsPlaceholder);
        return newEntries;
    }

    public List<Entry> GetEntriesWithoutPlaceholders()
    {
        var newEntries = new List<Entry>(entries);
        newEntries.RemoveAll(Entry.IsPlaceholder);
        return newEntries;
    }

    public bool IsEqual(Route other)
    {
        if (other == null || other.entries.Count != entries.Count)
        {
            return false;
        }

        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].id != other.entries[i].id || entries[i].alternateID != other.entries[i].alternateID)
            {
                return false;
            }
        }

        return true;
    }
}