using System;
using System.ComponentModel;
using System.Globalization;
using UnityEngine;

public class ScriptableObjectConverter<T> : TypeConverter where T : ScriptableObject
{
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		Quest quest = value as Quest;
		if (quest != null)
		{
			return new QuestSerializerPacket(quest.Filename, quest.ActiveStates, quest.QuestDescriptionID);
		}
		if (value is FlowChart)
		{
			return (value as FlowChart).Filename;
		}
		return (value as ScriptableObject).name;
	}

	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		T val = ScriptableObject.CreateInstance(typeof(T)) as T;
		if (val is Quest)
		{
			QuestSerializerPacket questSerializerPacket = value as QuestSerializerPacket;
			(val as Quest).Load(questSerializerPacket.Name);
			(val as Quest).ActiveStates = questSerializerPacket.ActiveStates;
			(val as Quest).QuestDescriptionID = questSerializerPacket.QuestDescriptionID;
		}
		if (val is FlowChart)
		{
			(val as FlowChart).Load(value as string);
		}
		return val;
	}
}
