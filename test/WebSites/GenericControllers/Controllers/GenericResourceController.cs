﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace GenericControllers.Controllers
{
    public abstract class GenericResourceController<TResource> where TResource : new()
    {
        /// <summary>
        /// Creates a resource
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        [HttpPost]
        public int Create([FromBody, Required]TResource resource)
        {
            return 1;
        }

        /// <summary>
        /// Retrieves all resources
        /// </summary>
        [HttpGet]
        public IEnumerable<TResource> Get()
        {
            return new[] { new TResource(), new TResource() };
        }

        /// <summary>
        /// Retrieves a specific resource
        /// </summary>
        [HttpGet("{id}")]
        public TResource Get(int id)
        {
            return new TResource();
        }

        [HttpPut("{id}")]
        public void Update(int id, [FromBody, Required]TResource resource)
        {
        }

        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}