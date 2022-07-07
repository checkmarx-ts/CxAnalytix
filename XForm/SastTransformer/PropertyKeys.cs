﻿using System;
using System.Collections.Generic;
using System.Text;

namespace CxAnalytix.XForm.SastTransformer
{
    /// <summary>
    /// Common property keys that can be re-used across record types
    /// </summary>
    internal class PropertyKeys
    {
        public static readonly String KEY_SCANID = "ScanId";
        public static readonly String KEY_SCANPRODUCT = "ScanProduct";
        public static readonly String KEY_SCANTYPE = "ScanType";
        public static readonly String KEY_SCANFINISH = "ScanFinished";
        public static readonly String KEY_SCANSTART = "ScanStart";
        public static readonly String KEY_SCANRISK = "ScanRisk";
        public static readonly String KEY_SCANRISKSEV = "ScanRiskSeverity";
        public static readonly String KEY_SIMILARITYID = "SimilarityId";
        public static readonly String KEY_ENGINESTART = "EngineStart";
        public static readonly String KEY_ENGINEFINISH = "EngineFinished";
    }
}
