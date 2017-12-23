﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EPiServer.Core;
using EPiServer.Shell.ObjectEditing;

namespace JOS.ContentSerializer.Internal
{
    public class StringPropertyHandler : PropertyHandler<string>
    {
        public override object Handle(object value, PropertyInfo property, IContentData contentData)
        {
            var stringValue = (string)value;
            if (HasSelectAttribute(property))
            {
                var selectOneAttribute = GetSelectOneAttribute(property);
                Type selectionFactoryType;
                if (selectOneAttribute != null)
                {
                    selectionFactoryType = selectOneAttribute.SelectionFactoryType;
                }
                else
                {
                    var selectManyAttribute = GetSelectManyAttribute(property);
                    selectionFactoryType = selectManyAttribute.SelectionFactoryType;
                }

                var valueAsDictionary = GetStructuredData(property, (IContentData)value, selectionFactoryType); // TODO fix contentdata
                return valueAsDictionary;
            }

            var propertyValue = property.GetValue(stringValue);
            return propertyValue;
        }

        private static bool HasSelectAttribute(PropertyInfo property)
        {
            var selectOne = GetSelectOneAttribute(property);
            if (selectOne != null) return true;

            var selectMany = GetSelectManyAttribute(property);
            return selectMany != null;
        }

        private static SelectOneAttribute GetSelectOneAttribute(PropertyInfo propertyInfo)
        {
            var attribute = (SelectOneAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(SelectOneAttribute));
            return attribute;
        }

        private static SelectManyAttribute GetSelectManyAttribute(PropertyInfo propertyInfo)
        {
            var selectMany = (SelectManyAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(SelectManyAttribute));
            return selectMany;
        }

        private static object GetStructuredData(PropertyInfo property, IContentData contentData, Type selectionFactoryType)
        {
            var selectOptions = GetSelectionOptions(selectionFactoryType, property);
            var propertyValue = (string)property.GetValue(contentData);
            return GetSelectOptions(propertyValue, selectOptions);
        }

        private static IEnumerable<ISelectItem> GetSelectionOptions(Type selectionFactoryType, object property)
        {
            var factory = (ISelectionFactory)Activator.CreateInstance(selectionFactoryType);
            var options = factory.GetSelections(property as ExtendedMetadata);
            return options;
        }

        private static IEnumerable<SelectOption> GetSelectOptions(string property, IEnumerable<ISelectItem> selectOptions)
        {
            var items = new List<SelectOption>();
            var selectedValues = property?.Split(',') ?? Enumerable.Empty<string>();

            foreach (var option in selectOptions)
            {
                var item = new SelectOption
                {
                    Selected = selectedValues.Contains(option.Value.ToString()),
                    Text = option.Text,
                    Value = option.Value.ToString()
                };

                items.Add(item);
            }

            return items;
        }
    }
}
