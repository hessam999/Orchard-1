﻿using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Environment.Extensions.Models;
using Orchard.Events;

namespace Orchard.DisplayManagement.Descriptors.ShapeTemplateStrategy {
    /// <summary>
    /// This service determines which paths to examine, and provides
    /// the shape type based on the template file paths discovered
    /// </summary>
    public interface IShapeTemplateHarvester : IDependency {
        IEnumerable<string> SubPaths();
        IEnumerable<HarvestShapeHit> HarvestShape(HarvestShapeInfo info);
    }

    public class BasicShapeTemplateHarvester : IShapeTemplateHarvester {
        private readonly IEnumerable<IShapeTemplateViewEngine> _shapeTemplateViewEngines;

        public BasicShapeTemplateHarvester(IEnumerable<IShapeTemplateViewEngine> shapeTemplateViewEngines) {
            _shapeTemplateViewEngines = shapeTemplateViewEngines;
        }

        public IEnumerable<string> SubPaths() {
            return new[] { "Views", "Views/Items", "Views/Parts", "Views/Fields" };
        }

        public IEnumerable<HarvestShapeHit> HarvestShape(HarvestShapeInfo info) {
            var lastDot = info.FileName.IndexOf('.');
            if (lastDot <= 0) {
                yield return new HarvestShapeHit {
                    ShapeType = Adjust(info.SubPath, info.FileName)
                };
            }
            else {
                yield return new HarvestShapeHit {
                    ShapeType = Adjust(info.SubPath, info.FileName.Substring(0, lastDot)),
                    DisplayType = info.FileName.Substring(lastDot + 1)
                };
            }
        }

        static string Adjust(string subPath, string fileName) {
            var leader="";
            if (subPath.StartsWith("Views/")) {
                leader = subPath.Substring("Views/".Length) + "_";
            }
            if (leader == "Items_" && !fileName.StartsWith("Content")) {
                leader = "Items_Content__";
            }

            // canonical shape type names must not have - or . to be compatible 
            // with display and shape api calls)))
            return leader + fileName.Replace("--", "__").Replace("-", "__").Replace('.', '_');
        }
    }

    public class HarvestShapeInfo {
        public string SubPath { get; set; }
        public string FileName { get; set; }
        public string TemplateVirtualPath { get; set; }
    }

    public class HarvestShapeHit {
        public string ShapeType { get; set; }
        public string DisplayType { get; set; }
    }

    public interface IShapeTemplateViewEngine : IDependency {
        IEnumerable<string> DetectTemplateFileNames(string virtualPath);
    }

}
