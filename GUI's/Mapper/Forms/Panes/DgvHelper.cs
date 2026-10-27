using Honeywell.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zuby.ADGV;

namespace Honeywell.GUI.Mapper
{
    //todo: filter/sort when dgv is empty fucks up
    internal class DgvHelper<T>
    {
        public static List<T> GetSortedDataSource(AdvancedDataGridView.SortEventArgs e, object datasource)
        {
            var currentDataSource = (List<T>)datasource;

            var sortString = e.SortString;
            if (string.IsNullOrWhiteSpace(sortString))
                return currentDataSource;

            var split = sortString.Split(',');
            foreach (var s in split)
            {
                var column = s[(s.IndexOf('[') + 1)..s.IndexOf(']')];
                var sort = s[(s.IndexOf(']') + 2)..];

                var prop = typeof(T).GetProperty(column);
                if (prop == null || string.IsNullOrWhiteSpace(sort))
                    return currentDataSource;

                if (sort == "ASC")
                    currentDataSource = currentDataSource.OrderBy(x => prop.GetValue(x)).ToList();
                else if (sort == "DESC")
                    currentDataSource = currentDataSource.OrderByDescending(x => prop.GetValue(x)).ToList();

                
            }
            return currentDataSource;
        }
        public static List<T> GetFilteredDataSource(AdvancedDataGridView.FilterEventArgs e, List<T> originalDataSource, object datasource)
        {
            if (originalDataSource == null)
                originalDataSource = (List<T>)datasource;

            List<T> newDatasource = new();

            var filterString = e.FilterString;
            if (string.IsNullOrWhiteSpace(filterString))
                newDatasource = originalDataSource;
            else
            {
                List<T> currentDataSource = originalDataSource.ToList(); //copy list

                var split = filterString.Split(" AND ");
                for (int k = 0; k < split.Length; k++)
                {
                    var s= split[k];
                    if (split.Length > 1)
                        s = s[1..^1];

                    var column = s[(s.IndexOf('[') + 1)..s.IndexOf(']')];
                    string? filter = null;
                    if (!s.Contains("IS NULL"))
                        filter = s[(s.IndexOf(')') + 2)..];

                    var prop = typeof(T).GetProperty(column);
                    if (prop == null)
                        continue;

                    if (filter == null)
                    {
                        foreach (var item in currentDataSource)
                        {
                            var value = prop.GetValue(item)?.ToString();
                            if (string.IsNullOrWhiteSpace(value))
                                newDatasource.Add(item);
                        }
                    }
                    else if (filter[..2] == "IN")
                    {
                        var criteria = filter[4..^2].RemoveLiterals().Split(',');
                        for (int i = 0; i < criteria.Length; i++)
                            criteria[i] = criteria[i].Trim().Trim('\'');

                        foreach (var item in currentDataSource)
                        {
                            var value = prop.GetValue(item)?.ToString();
                            if (criteria.Contains(value, StringComparison.OrdinalIgnoreCase))
                                newDatasource.Add(item);
                        }
                    }
                    else if (filter[..4] == "LIKE")
                    {
                        var criteria = filter[5..^1].Trim().Trim('\''); // contains
                        if (criteria[0] == '%' && criteria[^1] == '%')
                        {
                            criteria = criteria[1..^1].RemoveLiterals();
                            foreach (var item in currentDataSource)
                            {
                                var value = prop.GetValue(item)?.ToString();
                                if (value.Contains(criteria, StringComparison.OrdinalIgnoreCase))
                                    newDatasource.Add(item);
                            }
                        }
                        else if (criteria[0] == '%') //ends with
                        {
                            criteria = criteria[1..].RemoveLiterals();
                            foreach (var item in currentDataSource)
                            {
                                var value = prop.GetValue(item)?.ToString();
                                if (value.EndsWith(criteria, StringComparison.OrdinalIgnoreCase))
                                    newDatasource.Add(item);
                            }
                        }
                        else if (criteria[^1] == '%') //begins with
                        {
                            criteria = criteria[..^1].RemoveLiterals();
                            foreach (var item in currentDataSource)
                            {
                                var value = prop.GetValue(item)?.ToString();
                                if (value.StartsWith(criteria, StringComparison.OrdinalIgnoreCase))
                                    newDatasource.Add(item);
                            }
                        }
                        else //equals
                        {
                            criteria = criteria.RemoveLiterals();
                            foreach (var item in currentDataSource)
                            {
                                var value = prop.GetValue(item)?.ToString();
                                if (value.Equals(criteria, StringComparison.OrdinalIgnoreCase))
                                    newDatasource.Add(item);
                            }
                        }

                    }
                    else if (filter[..8] == "NOT LIKE")
                    {
                        var criteria = filter[9..^1].Trim().Trim('\''); // contains
                        if (criteria[0] == '%' && criteria[^1] == '%')
                        {
                            criteria = criteria[1..^1].RemoveLiterals();
                            foreach (var item in currentDataSource)
                            {
                                var value = prop.GetValue(item)?.ToString();
                                if (!value.Contains(criteria, StringComparison.OrdinalIgnoreCase))
                                    newDatasource.Add(item);
                            }
                        }
                        else if (criteria[0] == '%') //ends with
                        {
                            criteria = criteria[1..].RemoveLiterals();
                            foreach (var item in currentDataSource)
                            {
                                var value = prop.GetValue(item)?.ToString();
                                if (!value.EndsWith(criteria, StringComparison.OrdinalIgnoreCase))
                                    newDatasource.Add(item);
                            }
                        }
                        else if (criteria[^1] == '%') //begins with
                        {
                            criteria = criteria[..^1].RemoveLiterals();
                            foreach (var item in currentDataSource)
                            {
                                var value = prop.GetValue(item)?.ToString();
                                if (!value.StartsWith(criteria, StringComparison.OrdinalIgnoreCase))
                                    newDatasource.Add(item);
                            }
                        }
                        else //equals
                        {
                            criteria = criteria.RemoveLiterals();
                            foreach (var item in currentDataSource)
                            {
                                var value = prop.GetValue(item)?.ToString();
                                if (!value.Equals(criteria, StringComparison.OrdinalIgnoreCase))
                                    newDatasource.Add(item);
                            }
                        }
                    }
                    currentDataSource = newDatasource.ToList(); //copy filter list

                    if (k < split.Length - 1)
                        newDatasource.Clear();
                }
            }
            return newDatasource;
        }

    }
}
