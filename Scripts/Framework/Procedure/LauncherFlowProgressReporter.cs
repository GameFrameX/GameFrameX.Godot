using System;
using GameFrameX.Runtime;

namespace Godot.Startup.Procedure;

/// <summary>
/// 启动流程进度上报器，用于将状态机真实进度同步给 UILauncher。
/// </summary>
public static class LauncherFlowProgressReporter
{
	private static readonly object Gate = new();
	private static float _progress;
	private static string _stage = string.Empty;
	private static int _revision;
	private static bool _started;

	public readonly struct ProgressSnapshot
	{
		public ProgressSnapshot(float progress, string stage, int revision, bool started)
		{
			Progress = progress;
			Stage = stage ?? string.Empty;
			Revision = revision;
			Started = started;
		}

		public float Progress { get; }
		public string Stage { get; }
		public int Revision { get; }
		public bool Started { get; }
		public bool IsCompleted => Progress >= 100f;
	}

	public static void Begin(string stage)
	{
		lock (Gate)
		{
			_progress = 0f;
			_stage = stage ?? string.Empty;
			_revision++;
			_started = true;
			Log.Info("[LauncherProgress] begin stage={0} revision={1}", _stage, _revision);
		}
	}

	public static void Report(float progress, string stage = null)
	{
		lock (Gate)
		{
			if (_started == false)
			{
				_started = true;
				_revision++;
			}

			var nextProgress = Math.Clamp(progress, 0f, 100f);
			if (nextProgress + 0.001f < _progress)
			{
				return;
			}

			var hasStage = string.IsNullOrWhiteSpace(stage) == false;
			var stageChanged = hasStage && !string.Equals(_stage, stage, StringComparison.Ordinal);
			var progressChanged = Math.Abs(nextProgress - _progress) > 0.001f;
			if (progressChanged == false && stageChanged == false)
			{
				return;
			}

			_progress = nextProgress;
			if (hasStage)
			{
				_stage = stage;
			}

			Log.Info("[LauncherProgress] progress={0:F1}% stage={1}", _progress, _stage);
		}
	}

	public static void ReportRangeProgress(float rangeStart, float rangeEnd, float ratio, string stage = null)
	{
		var clampedRatio = Math.Clamp(ratio, 0f, 1f);
		var clampedStart = Math.Clamp(rangeStart, 0f, 100f);
		var clampedEnd = Math.Clamp(rangeEnd, 0f, 100f);
		var progress = clampedStart + (clampedEnd - clampedStart) * clampedRatio;
		Report(progress, stage);
	}

	public static ProgressSnapshot GetSnapshot()
	{
		lock (Gate)
		{
			return new ProgressSnapshot(_progress, _stage, _revision, _started);
		}
	}
}
