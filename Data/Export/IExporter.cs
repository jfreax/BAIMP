﻿using System;
using System.Collections.Generic;

namespace Baimp
{
	public interface IExporter
	{
		void Run(PipelineView pipeline);
	}
}
