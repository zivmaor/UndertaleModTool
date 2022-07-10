﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using static UndertaleModLib.Models.UndertaleGeneralInfo;

namespace UndertaleModLib.Models;

/// <summary>
/// A function entry as it's used in a GameMaker data file.
/// </summary>
[PropertyChanged.AddINotifyPropertyChangedInterface]
public class UndertaleFunction : UndertaleNamedResource, UndertaleInstruction.ReferencedObject, IDisposable
{
    public FunctionClassification Classification { get; set; }

    /// <summary>
    /// The name of the <see cref="UndertaleFunction"/>.
    /// </summary>
    public UndertaleString Name { get; set; }

    /// <summary>
    /// The index of <see cref="Name"/> in <see cref="UndertaleData.Strings"/>.
    /// </summary>
    public int NameStringID { get; set; }

    /// <summary>
    /// How often this <see cref="UndertaleFunction"/> is referenced in code.
    /// </summary>
    public uint Occurrences { get; set; }
    public UndertaleInstruction FirstAddress { get; set; }

    public bool Autogenerated { get; set; } = false; // Whether a gml_Script_... kind of function in 2.3

    [Obsolete("This variable has been renamed to NameStringID.")]
    public int UnknownChainEndingValue { get => NameStringID; set => NameStringID = value; }

    /// <inheritdoc />
    public void Serialize(UndertaleWriter writer)
    {
        writer.WriteUndertaleString(Name);
        writer.Write(Occurrences);
        if (Occurrences > 0)
        {
            uint addr = writer.GetAddressForUndertaleObject(FirstAddress);
            if (writer.undertaleData.IsVersionAtLeast(2, 3))
                writer.Write((addr == 0) ? 0 : (addr + 4)); // in GMS 2.3, it points to the actual reference rather than the instruction
            else
                writer.Write(addr);
        }
        else
            writer.Write((int)-1);
    }

    /// <inheritdoc />
    public void Unserialize(UndertaleReader reader)
    {
        Name = reader.ReadUndertaleString();
        Occurrences = reader.ReadUInt32();
        if (Occurrences > 0)
        {
            if (reader.GMS2_3)
                FirstAddress = reader.GetUndertaleObjectAtAddress<UndertaleInstruction>(reader.ReadUInt32() - 4);
            else
                FirstAddress = reader.ReadUndertaleObjectPointer<UndertaleInstruction>();
            UndertaleInstruction.Reference<UndertaleFunction>.ParseReferenceChain(reader, this);
        }
        else
        {
            if (reader.ReadInt32() != -1)
                throw new Exception("Function with no occurrences, but still has a first occurrence address");
            FirstAddress = null;
        }
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Name?.Content;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        GC.SuppressFinalize(this);

        Name = null;
        FirstAddress = null;
    }
}

// Seems to be unused. You can remove all entries and the game still works normally.
// Maybe the GM:S debugger uses this data?
// TODO: INotifyPropertyChanged
public class UndertaleCodeLocals : UndertaleNamedResource, IDisposable
{
    public UndertaleString Name { get; set; }
    public ObservableCollection<LocalVar> Locals { get; } = new ObservableCollection<LocalVar>();

    /// <inheritdoc />
    public void Serialize(UndertaleWriter writer)
    {
        writer.Write((uint)Locals.Count);
        writer.WriteUndertaleString(Name);
        foreach (LocalVar var in Locals)
        {
            writer.WriteUndertaleObject(var);
        }
    }

    /// <inheritdoc />
    public void Unserialize(UndertaleReader reader)
    {
        uint count = reader.ReadUInt32();
        Name = reader.ReadUndertaleString();
        Locals.Clear();
        for (uint i = 0; i < count; i++)
        {
            Locals.Add(reader.ReadUndertaleObject<LocalVar>());
        }
        Util.DebugUtil.Assert(Locals.Count == count);
    }

    public bool HasLocal(string varName)
    {
        return Locals.Any(local=>local.Name.Content == varName);
    }

    // TODO: INotifyPropertyChanged
    public class LocalVar : UndertaleObject, IDisposable
    {
        public uint Index { get; set; }
        public UndertaleString Name { get; set; }

        /// <inheritdoc />
        public void Serialize(UndertaleWriter writer)
        {
            writer.Write(Index);
            writer.WriteUndertaleString(Name);
        }

        /// <inheritdoc />
        public void Unserialize(UndertaleReader reader)
        {
            Index = reader.ReadUInt32();
            Name = reader.ReadUndertaleString();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            GC.SuppressFinalize(this);

            Name = null;
        }
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Name?.Content + " (" + GetType().Name + ")";
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        GC.SuppressFinalize(this);

        Name = null;
        Locals.Clear();
    }
}