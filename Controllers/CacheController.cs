using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Linq;

namespace StudingMemoryCache.Controllers
{
    // 等待研究用途
    // CacheItemPolicy 

    [Route("[controller]")]
    [ApiController]
    public class CacheController : ControllerBase
    {
        private readonly IMemoryCache cache;

        private readonly MemoryCache memoryCache; // 研究中，這個似乎可以直接用？

        public CacheController(IMemoryCache cache)
        {
            this.cache = cache;
            memoryCache = (MemoryCache)cache;
        }

        /// <summary>
        /// 查看所有快取資料
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult ShowAll()
        {
            return Ok(ShowAllCache());
        }

        /// <summary>
        /// 查看指定快取
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [HttpGet("{key}")]
        public IActionResult ShowKey(string key)
        {
            // if no key, then get null
            var result = cache.Get(key);
            return Ok(result);
        }

        /// <summary>
        /// 新增資料 (永久)
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Forever(string key)
        {
            cache.Set(key, key);

            // 研究 IMemoryCache & MemoryCache 的資料差異
            return Ok(new {
                //IMemoryCacheCount = cache.Get(key), // 不知道怎麼查看 IMemoryCache 資料量
                IMemoryCacheData = cache.Get(key),
                MemoryCacheCount = memoryCache.Count,
                MemoryCacheData = memoryCache.Get(key),
            });
        }

        /// <summary>
        /// 研究快取截止時間自動釋放
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public IActionResult AbsoluteExpiration(string key, int seconds)
        {
            //var cacheData = _cache.Get<HashSet<string>>(keepNewWords) ?? new HashSet<string>();

            // 用到這個 Key 的時候才會判斷是否到期
            // 若用 cache.Get() 發現此 key 到期，就會刪除返回 null
            cache.Set(key, key, DateTimeOffset.Now.AddSeconds(seconds));
            //_cache.Set(keepNewWords, cacheData, TimeSpan.FromSeconds(absoluteExpiration));

            return Ok(JsonConvert.SerializeObject(cache.Get(key)));
        }

        [HttpDelete]
        public IActionResult Delete(string key)
        {
            // 刪除不存在的 key，不會報錯
            cache.Remove(key);

            var result = cache.Get(key);

            if (result == null)
                return Ok();
            else
                return StatusCode((int)HttpStatusCode.InternalServerError, new
                {
                    message = $"Error: Delete Key"
                });
        }



        private object ShowAllCache()
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var entries = cache.GetType().GetField("_entries", flags).GetValue(cache);
            var cacheItems = entries as System.Collections.IDictionary;
            var keys = new List<string>();

            if (cacheItems == null)
                return keys;

            foreach (System.Collections.DictionaryEntry cacheItem in cacheItems)
            {
                object value = cacheItem.Value;
                string valueString = cacheItem.Value.ToString(); // 這個 value 本身也是個 Dictionary 的樣子，所以還要再解 可能才能看到值

                keys.Add($"{cacheItem.Key} : {cacheItem.Value} ");   // 這樣取不到值
            }
            return keys;
        }
    }
}
