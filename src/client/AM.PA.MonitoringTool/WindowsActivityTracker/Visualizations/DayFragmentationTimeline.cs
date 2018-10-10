// Created by André Meyer (ameyer@ifi.uzh.ch) from the University of Zurich
// Created: 2017-01-03
// 
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Shared;
using Shared.Helpers;
using WindowsActivityTracker.Data;
using WindowsActivityTracker.Models;
using ActivityMapper;

namespace WindowsActivityTracker.Visualizations
{
    internal class DayFragmentationTimeline : BaseVisualization, IVisualization
    {
        private readonly DateTimeOffset _date;
        private const int TimelineZoomFactor = 1; // shouldn't be 0!, if > 1 then the user can scroll on top of the timeline
        private StringBuilder _sb = new StringBuilder();

        public DayFragmentationTimeline(DateTimeOffset date)
        {
            this._date = date;

            Title = "Timeline: Activities over the Day"; //hint; overwritten below
            IsEnabled = true; //todo: handle by user
            Order = 2; //todo: handle by user
            Size = VisSize.Wide;
            Type = VisType.Day;
        }

        public override string GetHtml()
        {
            var html = string.Empty;

            /////////////////////
            // fetch data sets
            /////////////////////
            var orderedTimelineList = Queries.GetDayTimelineData(_date);

            /////////////////////
            // data cleaning
            /////////////////////

            // show message if not enough data
            var sum = orderedTimelineList.Sum(i => i.DurationInSeconds);
            if (orderedTimelineList.Count <= 3 || sum < 5 * 60) // 3 is the minimum number of input-data-items & 5 minutes of data
            {
                _sb.Append(VisHelper.NotEnoughData(Dict.NotEnoughData));
                return _sb.ToString();
            }

            // remove first + last items if IDLE
            if (orderedTimelineList.First().ActivityCategory == ActivityCategory.Idle)
            {
                orderedTimelineList.Remove(orderedTimelineList.First());
            }
            if (orderedTimelineList.Last().ActivityCategory == ActivityCategory.Idle)
            {
                orderedTimelineList.Remove(orderedTimelineList.Last());
            }

            /////////////////////
            // Create HTML
            /////////////////////

            _sb.Append(GetActivityVisualizationContent(orderedTimelineList));
            return _sb.ToString();
        }

        private string GetActivityVisualizationContent(List<WindowsActivity> activityList)
        {
            var categories = activityList.Select(a => a.ActivityCategory).Distinct().OrderBy(a => a).ToList();
            const string activityTimeline = "activityTimeline";
            const string defaultHoverText = "Hint: Hover over the timeline to see details.";

            var html = string.Empty;

            /////////////////////
            // CSS
            /////////////////////

            html += @"<style type='text/css'>
                    .axis path,
                    .axis line {
                      fill: none;
                      stroke: black;
                      shape-rendering: crispEdges;
                    }
                    .axis text {
                      font-size: .71em;
                    }
                    .timeline-label {
                      font-size: .71em;
                    }
                    /*.coloredDiv {
                      height:20px; width:20px; float:left; margin-right:5px;
                    }
                    </style>";

            /////////////////////
            // Javascript
            /////////////////////

            html += @"<script type='text/javascript'>
                    var onLoad = window.onload;
                    window.onload = function() { 
                    if (typeof onLoad == 'function') { onLoad(); } ";

            // create formatted javascript data list
            html += "var data = [" + CreateJavascriptActivityDataList(activityList) + "]; ";

            // create color scale
            html += CreateColorScheme(categories);

            // width & height
            html += "var itemWidth = 0.98 * document.getElementsByClassName('item Wide')[0].offsetWidth;";
            html += "var itemHeight = 0.13 * document.getElementsByClassName('item Wide')[0].offsetHeight;";

            // hover Event (d: current rendering object, i: index during d3 rendering, data: data object)
            const string hover = @".hover(function(d, i, data) { 
                                    document.getElementById('hoverDetails').innerHTML = '<span style=\'font-size:1.2em; color:#007acc;\'>From ' + d['starting_time_formatted'] + ' to ' + d['ending_time_formatted'] + ' (' + d['duration'] + 'min)</span>' +
                                                                                        '<br /><strong>Activity</strong>: <span style=\'color:' + d['color'] + '\'>■</span> ' + d['activity'] +
                                                                                        '<br /><strong>Processes</strong>: ' + d['processes'] + 
                                                                                        '<br /><strong>Window Titles</strong>: ' + d['window_titles']; 
                                })";

            // mouseout Event
            const string mouseout = @".mouseout(function (d, i, datum) { document.getElementById('hoverDetails').innerHTML = '" + defaultHoverText + "'; })";

            // define configuration
            html += "var " + activityTimeline + " = d3.timeline().width(" + TimelineZoomFactor + " * itemWidth).itemHeight(itemHeight)" + hover + mouseout + ";"; // .colors(colorScale).colorProperty('activity') // .stack()
            html += "var svg = d3.select('#" + activityTimeline + "').append('svg').attr('width', itemWidth).datum(data).call(" + activityTimeline + "); ";

            html += "}; "; // end #1
            html += "</script>";

            /////////////////////
            // HTML
            /////////////////////

            // show details on hover
            html += "<div style='height:37%; style='align: center'><p id='hoverDetails'>" + defaultHoverText + "</p></div>";

            // add timeline
            html += "<div id='" + activityTimeline + "' align='center'></div>";

            // add legend 
            html += GetLegendForCategories(categories);

            return html;
        }

        /// <summary>
        /// prepares a formatted javascript list of the participantActivityData
        /// </summary>
        /// <param name="activityList"></param>
        /// <returns></returns>
        private string CreateJavascriptActivityDataList(List<WindowsActivity> activityList)
        {
            var html = string.Empty;

            var categories = activityList.Select(a => a.ActivityCategory).Distinct().ToList();

            foreach (var category in categories)
            {
                //var times = string.Empty;
                var times = new StringBuilder();
                foreach (var activityEntry in activityList.Where(a => a.ActivityCategory == category))
                {
                    var startTime = JavascriptTimestampFromDateTime(activityEntry.StartTime);
                    var endTime = JavascriptTimestampFromDateTime(activityEntry.EndTime);

                    // add data used for the timeline and the timeline hover
                    times.Append("{");
                    times.Append("'starting_time': "); times.Append(startTime);
                    times.Append(", 'ending_time': "); times.Append(endTime);
                    times.Append(", 'starting_time_formatted': '"); times.Append(activityEntry.StartTime.ToShortTimeString());
                    times.Append("', 'ending_time_formatted': '"); times.Append(activityEntry.EndTime.ToShortTimeString());
                    times.Append("', 'duration': "); times.Append(Math.Round(activityEntry.DurationInSeconds / 60.0, 1));
                    times.Append(", 'window_titles': '"); times.Append(ReadableWindowTitles(activityEntry.WindowProcessList));
                    times.Append("', 'processes': '"); times.Append(ReadableProcesses(activityEntry.WindowProcessList));
                    times.Append("', 'color': '"); times.Append(GetHtmlColorForContextCategory(activityEntry.ActivityCategory));
                    times.Append("', 'activity': '"); times.Append(activityEntry.ActivityCategory.GetDescription());
                    times.Append("'}, ");
                }

                html += "{activity: '" + category + "', times: [" + times.ToString() + "]}, ";
            }

            return html;
        }

        #region Readable WindowTitle and Process

        private static string ReadableWindowTitles(List<WindowProcessItem> list)
        {
            const int maxNumItems = 4;
            var str = string.Empty;

            // distinct items
            var windowTitles = list.Select(i => i.WindowTitle).Distinct().ToList();

            // only maxNumItems
            if (windowTitles.Count > maxNumItems)
            {
                for (var i = 0; i < maxNumItems; i++)
                {
                    str += FormatWindowTitle(windowTitles[i]);
                }
                str += " and " + (windowTitles.Count - maxNumItems) + " more.";
            }
            else
            {
                str = windowTitles.Aggregate(str, (current, item) => current + FormatWindowTitle(item));
            }
            return str.Trim().TrimEnd(',');
        }

        private static string FormatWindowTitle(string windowTitle)
        {
            return string.IsNullOrEmpty(windowTitle) 
                ? string.Empty 
                : windowTitle.Replace("'", "").Replace("'", "").Replace("/", "//").Replace(@"\", @"\\").Replace("\r\n", "").Replace("\t", "").Replace("\r", "").Replace("\n", "") + ", ";
        }

        private static string ReadableProcesses(List<WindowProcessItem> list)
        {
            // distinct processes
            var processes = list.Select(i => i.Process).Distinct().ToList();

            // build readable string
            var str = string.Empty;
            foreach (var item in processes) str += FormatProcesses(item);
            return str.Trim().TrimEnd(',');
        }

        private static string FormatProcesses(string process)
        {
            return string.IsNullOrEmpty(process) ? string.Empty : ProcessNameHelper.GetFileDescription(process).Replace("'", "") + ", ";
        }

        #endregion

        #region Helpers for legend and colors

        /// <summary>
        /// Creates a colored square for each category (legend)
        /// </summary>
        /// <param name="categoryList"></param>
        /// <returns></returns>
        private string GetLegendForCategories(IEnumerable<ActivityCategory> categoryList)
        {
            var html = string.Empty;
            html += @"<style type='text/css'>
                    #legend li { display: inline-block; padding-right: 1em; list-style-type: square; }
                    li:before { content: '■ '} 
                    li span { font-size: .71em; color: black;} 
                    </style>";

            html += "<div><ul id='legend' align='center'>" // style='width:" + visWidth + "px'
                   +  categoryList.Where(c => c != ActivityCategory.Idle).Aggregate(string.Empty, (current, cat) => current + ("<li style='color:" + GetHtmlColorForContextCategory(cat) + "'><span>" + cat.GetDescription() + "</span></li>"))
                   +  "</ul></div>";

            return html;
        }

        //private string GetLegendEntryForActivity(ActivityCategory category)
        //{
        //    return "<li style='color:" + GetHtmlColorForContextCategory(category) + "'><span>" + GetDescriptionForContextCategory(category) + "</span></li>";
        //}

        /// <summary>
        /// Creates a colorscheme for each activity category
        /// </summary>
        /// <param name="categories"></param>
        /// <returns></returns>
        private string CreateColorScheme(List<ActivityCategory> categories)
        {
            var rangeString = categories.Aggregate(string.Empty, (current, item) => current + ("'" + GetHtmlColorForContextCategory(item) + "', "));
            var activityString = categories.Aggregate(string.Empty, (current, item) => current + ("'" + item + "', "));

            var html = "var colorScale = d3.scale.ordinal().range([" + rangeString + "]).domain([" + activityString + "]); ";

            return html;
        }

        /// <summary>
        /// Return a color for each context category
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        private static string GetHtmlColorForContextCategory(ActivityCategory category)
        {
            const string noneColor = "#DDDDDD";

            switch (category)
            {
                case ActivityCategory.DevCode:
                    return "#A547D1";//"#6B238E";//"#00b300"; //"#007acb"; // darker blue
                case ActivityCategory.DevDebug:
                    return "#C91BC7";//"#8EC4E8"; // fairest blue
                case ActivityCategory.DevReview:
                    return "#D7ADEB"; //"#1484CE"; //fairest blue
                case ActivityCategory.DevVc:
                    return "#F9D1F8";// "#1484CE"; //fairest blue  
                case ActivityCategory.ReadWriteDocument:
                case ActivityCategory.PlannedMeeting:
                    return "#99EBFF";//#87CEEB";// "#3258E6";//  "#00b300"; // dark green
                case ActivityCategory.InformalMeeting:
                case ActivityCategory.InstantMessaging:
                    return "#12A5F4";// "#C91BC7";//"#00cc00"; // fair green
                case ActivityCategory.Planning:
                    return "#9DB7E8"; // "#F9D1F8";//"#e855e8"; // dark violett
                case ActivityCategory.Email:
                    return "#326CCC";// "#2858a5";//"#ED77EB"; // fair violett
                //case ActivityCategory.WebBrowsing:
                case ActivityCategory.WorkRelatedBrowsing:
                    return "#FF9333"; //orange "#FFE3CB";//"#FFA500"; 
                case ActivityCategory.WorkUnrelatedBrowsing:
                    return "#FFC999"; // "red"; // fair orange
                case ActivityCategory.FileNavigationInExplorer:
                    return "#d3d3d3"; // light gray
                case ActivityCategory.Other:
                case ActivityCategory.OtherRdp:
                case ActivityCategory.Unknown:
                    return "gray";
                case ActivityCategory.Idle:
                    return "white";
                case ActivityCategory.Uncategorized:
                    return noneColor; 
            }

            return noneColor; // default color
        }

        #endregion

        #region Other Helpers

        private static long JavascriptTimestampFromDateTime(DateTime date)
        {
            var datetimeMinTimeTicks = (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).Ticks;
            return ((date.ToUniversalTime().Ticks - datetimeMinTimeTicks) / 10000);
            // return (date.Ticks - 621355968000000000)/10000; //old: had wrong timezone
        }

        #endregion
    }
}
