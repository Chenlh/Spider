﻿using D.Spider.Core.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using D.Spider.Core.Events;
using D.Util.Interface;
using System.Timers;

namespace D.Spider.Core
{
    /// <summary>
    /// IUrlManager 的实现
    /// </summary>
    public class UrlManager : IUrlManager
    {
        IEventBus _eventBus;
        ILogger _logger;

        List<IUrl> _allUrl;
        List<IUrl> _waitingCrawlUrl;
        List<IUrl> _crawlingUrl;

        Timer _urlCheckTimer;//定时检测是否有 Url 需要检测
        const int _urlCheckInterval = 10000;//定时检测的时间间隔

        public UrlManager(IEventBus eventBus, ILoggerFactory loggerFactory)
        {
            _eventBus = eventBus;
            _logger = loggerFactory.CreateLogger<UrlManager>();

            _urlCheckTimer = new Timer(_urlCheckInterval);
            _urlCheckTimer.Elapsed += new ElapsedEventHandler(UrlCheckTimer_Elapsed);

            _urlCheckTimer.Start();

            _allUrl = new List<IUrl>();
            _waitingCrawlUrl = new List<IUrl>();
            _crawlingUrl = new List<IUrl>();

            _eventBus.Subscribe(this);
        }

        ~UrlManager()
        {
            _urlCheckTimer.Stop();

            _allUrl.Clear();
            _waitingCrawlUrl.Clear();
            _crawlingUrl.Clear();
        }

        #region IUrlManager 接口实现

        public IUrl AddUrl(IUrl url)
        {
            var find = _allUrl.Find(uu => uu.Equal(url));

            _logger.LogInformation("向 UrlManager 中添加 Url " + url.UrlString);

            if (find == null)
            {
                _allUrl.Add(url);

                find = url;
            }
            else
            {
                find.Interval = url.Interval;
            }

            if (url.NeedCrawl() && !IsWaitingCrawl(url))
            {
                _waitingCrawlUrl.Add(find);

                _eventBus.Publish(new UrlWaitingEvent());
            }

            return find;
        }

        public IUrl AddUrl(string url)
        {
            if (Url.IsUrl(url))
            {
                return AddUrl(new Url(url));
            }
            else
            {
                _logger.LogError(url + " 不是正确的 url");
                return null;
            }
        }

        public IUrl AddUrl(string host, string relativePath)
        {
            if (Url.IsUrl(host))
            {
                return AddUrl(new Url(host, relativePath));
            }
            else
            {
                _logger.LogError(host + " 不是正确的 url 主机地址");
                return null;
            }
        }

        public bool IsWaitingCrawl(IUrl url)
        {
            return _waitingCrawlUrl.Find(uu => uu.Equal(url)) != null;
        }

        public IUrl NextCrawl()
        {
            lock (this)
            {
                var nextUrl = _waitingCrawlUrl.FirstOrDefault();

                if (nextUrl != null)
                {
                    _waitingCrawlUrl.RemoveAt(0);
                    _crawlingUrl.Add(nextUrl);
                }

                return nextUrl;
            }
        }

        public void Handle(UrlCrawledEvent e)
        {
            lock (this)
            {

                var findIndex = _crawlingUrl.FindIndex(u => u.Equal(e.Page.Url));
                if (findIndex > -1)
                {
                    _crawlingUrl.RemoveAt(findIndex);
                }
            }
        }

        #endregion

        /// <summary>
        /// url 定时检测函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UrlCheckTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _logger.LogInformation("定时检测是否有 Url 需要爬取");

            var urlNeedCrawlCount = 0;

            foreach (var url in _allUrl)
            {
                if (url.NeedCrawl() && !IsWaitingCrawl(url))
                {
                    _waitingCrawlUrl.Add(url);

                    urlNeedCrawlCount++;
                }
            }

            _logger.LogInformation("需要爬取的 Url 数量 " + urlNeedCrawlCount);

            if (urlNeedCrawlCount > 0)
            {
                _logger.LogDebug("发布事件 UrlWaitingEvent");
                _eventBus.Publish(new UrlWaitingEvent());
            }
        }
    }
}