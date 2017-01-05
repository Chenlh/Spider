﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D.NovelCrawl.Core.Models.DTO
{
    /// <summary>
    /// 小说爬取 Url 列表
    /// </summary>
    public class NovelCrawlUrlModel
    {
        /// <summary>
        /// Url
        /// </summary>
        /// <returns></returns>
        public string Url { get; set; }

        /// <summary>
        /// 是否是官网
        /// </summary>
        /// <returns></returns>
        public bool Official { get; set; }

        /// <summary>
        /// 页面的处理步骤
        /// </summary>
        public PageProcessStep ProcessStep { get; set; }
    }
}
