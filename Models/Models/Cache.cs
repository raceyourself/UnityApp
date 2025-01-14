﻿using System;
using System.IO;

namespace RaceYourself.Models
{
	public class Cache
	{
		public string id;
		public DateTime expiration;
        public string lastModified;

		public Cache() {}
        public Cache(string id, long maxAge) : this(id, maxAge, null)
        {
        } 
        public Cache(string id, long maxAge, string lastModified)
		{
			this.id = id;
			this.expiration = DateTime.Now.AddSeconds(maxAge);
            this.lastModified = lastModified;
		}

		public bool Expired {
			get {
				return (DateTime.Now > expiration);
			}
		}
	}
}

