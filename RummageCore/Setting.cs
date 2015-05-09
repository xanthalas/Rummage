using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RummageCore
{
    public class Setting
    {
        private bool boolean;
        private string text;
        private int integer;
        private List<string> collection;

        /// <summary>
        /// Allow settings to be serialized
        /// </summary>
        /// <remarks>Prevents exceptions from being thrown during serialization</remarks>
        public static bool AllowSerialize = false;

        /// <summary>
        /// Gets the name of this setting
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the description of this setting
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Gets the category of this setting
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// The type of this setting
        /// </summary>
        public SettingType Type { get; private set; }

        /// <summary>
        /// Gets this setting's value object
        /// </summary>
        public object Value { 
            get
            {
                switch (Type)
                {
                    case SettingType.boolean:
                        return boolean;
                        
                    case SettingType.text:
                        return text;

                    case SettingType.integer:
                        return integer;

                    case SettingType.collection:
                        return collection;

                    default:
                        throw new ArgumentException("Unknown setting type encountered: " + Type.ToString());
                }
            }
        }

        /// <summary>
        /// Create a new Setting
        /// </summary>
        /// <param name="name">The name of this setting</param>
        /// <param name="description">The description of this setting. Useful for displaying to the user on the preferences screen</param> 
        /// <param name="category">The category of this setting</param>
        /// <param name="type">The type of this setting</param>
        /// <param name="value">The value of this setting</param>
        public Setting(string name, string description, string category, SettingType type, object value)
        {
            Name = name;
            Description = description;
            Category = category;

            switch (type)
            {
                case SettingType.boolean:
                    if (value is bool)
                    {
                        boolean = (bool)value;
                    }
                    else
                    {
                        if (!Setting.AllowSerialize) throw new ArgumentException("Setting of type \"boolean\" was passed an incorrect value");
                    }
                    break;

                case SettingType.text:
                    if (value is string)
                    {
                        text = (string)value;
                    }
                    else
                    {
                        if (!Setting.AllowSerialize) throw new ArgumentException("Setting of type \"text\" was passed an incorrect value");
                    }
                    break;

                case SettingType.integer:
                    if (value is int)
                    {
                        integer = (int)value;
                    }
                    else
                    {
                        if (value is long)
                        {
                            long longValue = (long)value;
                            integer = (int)longValue;
                        }
                        else
                        {
                            if (!Setting.AllowSerialize) throw new ArgumentException("Setting of type \"integer\" was passed an incorrect value");
                        }
                    }
                    break;

                case SettingType.collection:
                    if (value is List<string>)
                    {
                        //collection = (List<string>)value;
                        collection = new List<string>();
                        foreach(var item in (List<string>)value)
                        {
                            collection.Add(item);
                        }
                        break;
                    }

                    if (value is List<string> || value is Newtonsoft.Json.Linq.JArray)
                    {
                        //collection = (List<string>)value;
                        collection = new List<string>();
                        foreach (var item in (Newtonsoft.Json.Linq.JArray)value)
                        {
                            //Strip off the leading and trailing \" characters added by the serializer
                            string stringToAdd = item.ToString();
                            if (stringToAdd.Substring(0,1) == "\"")
                            {
                                stringToAdd = stringToAdd.Substring(1);
                            }
                            
                            if (stringToAdd.Substring(stringToAdd.Length -1, 1) == "\"")
                            {
                                stringToAdd = stringToAdd.Substring(0, stringToAdd.Length - 1);
                            }
                            collection.Add(stringToAdd);

                        }
                        break;
                    }
                    
                    if (!Setting.AllowSerialize) throw new ArgumentException("Setting of type \"collection\" was passed an incorrect value");
                    break;

                default:
                    if (!Setting.AllowSerialize) throw new ArgumentException("Unknown setting type encountered: " + value.GetType().ToString());
                    break;
            }

            Type = type;
        }

        /// <summary>
        /// Gets the value of this setting as an integer. Throws a FormatException if this is not an integer setting object.
        /// </summary>
        public int ValueAsInteger
        {
            get
            {
                if (Type == SettingType.integer)
                {
                    return (int)Value;
                }
                else
                {
                    if (!Setting.AllowSerialize)
                    {
                        throw new FormatException(string.Format("Tried to access a setting of type {0} as an integer", Type.ToString()));
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the value of this setting as a boolean. Throws a FormatException if this is not a boolean setting object.
        /// </summary>
        public bool ValueAsBoolean
        {
            get
            {
                if (Type == SettingType.boolean)
                {
                    return (bool)Value;
                }
                else
                {
                    if (!Setting.AllowSerialize)
                    {
                        throw new FormatException(string.Format("Tried to access a setting of type {0} as a boolean", Type.ToString()));
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            set
            {
                if (Type == SettingType.boolean)
                {
                    boolean = value;
                }
            }
        }

        /// <summary>
        /// Gets/Sets the value of this setting as a string. Throws a FormatException if this is not a text setting object.
        /// </summary>
        public string ValueAsText
        {
            get
            {
                if (Type == SettingType.text)
                {
                    return (string)Value;
                }
                else
                {
                    if (!Setting.AllowSerialize)
                    {
                        throw new FormatException(string.Format("Tried to access a setting of type {0} as text", Type.ToString()));
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
            }
            set
            {
                if (Type == SettingType.text)
                {
                    text = value;
                }
            }
        }

        /// <summary>
        /// Gets the value of this setting as a collection. Throws a FormatException if this is not a collection setting object.
        /// </summary>
        public List<string> ValueAsCollection
        {
            get
            {
                if (Type == SettingType.collection)
                {
                    return Value as List<string>;
                }
                else
                {
                    if (!Setting.AllowSerialize)
                    {
                        throw new FormatException(string.Format("Tried to access a setting of type {0} as collection", Type.ToString()));
                    }
                    else
                    {
                        return new List<string>();
                    }
                }
            }
        }
    }
}
