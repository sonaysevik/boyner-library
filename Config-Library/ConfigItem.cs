using System;


namespace ConfigLibrary
{
    public class ConfigItem<T>
    {
        T value;

        public ConfigItem(T configValue)
        {
            this.value = configValue;
        }

        public T GetValue()
        {
            return this.value;
        }
    }
}
