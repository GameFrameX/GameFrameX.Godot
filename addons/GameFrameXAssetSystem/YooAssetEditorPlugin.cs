#if TOOLS
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Godot;
using YooAsset;

[Tool]
public partial class YooAssetEditorPlugin : EditorPlugin
{
    private const string HomePageMenu = "YooAsset/Home Page";
    private const string BuilderMenu = "YooAsset/AssetBundle Builder";
    private const string CollectorMenu = "YooAsset/AssetBundle Collector";
    private const string ReporterMenu = "YooAsset/AssetBundle Reporter";
    private const string DebuggerMenu = "YooAsset/AssetBundle Debugger";
    private const int BuilderTabIndex = 0;
    private const int CollectorTabIndex = 1;
    private const int ReporterTabIndex = 2;
    private const int DebuggerTabIndex = 3;
    private const int DefaultTabIndex = BuilderTabIndex;
    private const int BuilderScanLimit = 20000;
    private const string ManifestUnavailableNone = "None";
    private const string ManifestUnavailableNotFound = "ManifestNotFound";
    private const string ManifestUnavailableNoBuildRecord = "NoBuildRecord";
    private const string ReporterExportStateLogPrefix = "ExportState";
    private const string ReporterExportStateLogFormat = "{0}: {1}={2}, {3}={4}";
    private const string ExportFieldExportFileName = "ExportFileName";
    private const string ExportFieldExportTime = "ExportTime";
    private const string ExportFieldBuildManifestAvailable = "BuildManifestAvailable";
    private const string ExportFieldBuildManifestUnavailableReason = "BuildManifestUnavailableReason";
    private const string ExportSectionInfo = "[ExportInfo]";
    private const string ExportSectionReporterSummary = "[ReporterSummary]";
    private const string ExportSectionBuilderSnapshot = "[BuilderSnapshot]";
    private const string ExportSectionBuildManifest = "[BuildManifest]";
    private const string SnapshotFieldState = "State";
    private const string SnapshotFieldPackageName = "PackageName";
    private const string SnapshotFieldPipeline = "Pipeline";
    private const string SnapshotFieldBuildTime = "BuildTime";
    private const string SnapshotFieldOutputDirectory = "OutputDirectory";
    private const string SnapshotFieldManifestPath = "ManifestPath";
    private const string SnapshotFieldScannedCount = "ScannedCount";
    private const string SnapshotFieldFileCount = "FileCount";
    private const string SnapshotFieldScanLimitHit = "ScanLimitHit";
    private const string SnapshotFieldManifestAvailable = "ManifestAvailable";
    private const string SnapshotFieldManifestUnavailableReason = "ManifestUnavailableReason";
    private const string ExportFileNamePrefix = "report";
    private const string ExportFileNameSeparator = "_";
    private const string ExportFileNameExtension = ".txt";
    private const string ExportFileNameUnknownPackage = "unknown_package";
    private const string ExportFileNameTimestampFormat = "yyyyMMdd_HHmmss";
    private const char FileNameSpaceChar = ' ';
    private const char FileNameReplacementChar = '_';
    private const string ReporterExportOutputDirectory = "user://yooasset_reports";
    private const string ReporterExportStatusNoSummary = "Reporter 导出失败：请先执行报告。";
    private const string ReporterExportLogNoSummary = "导出失败：尚未生成报告。";
    private const string ReporterExportLogSuccessPrefix = "导出完成：";
    private const string ReporterExportStatusSuccess = "Reporter 导出占位执行完成。";
    private const string ReporterExportStatusFailed = "Reporter 导出失败。";
    private const string ReporterExportLogFailedPrefix = "导出失败：";
    private const string ReporterRunLogStart = "开始执行 Reporter 原型...";
    private const string ReporterRunLogScanRootPrefix = "Scan Root: ";
    private const string ReporterRunLogPipelinePrefix = "Pipeline: ";
    private const string ReporterRunLogFilterKeywordPrefix = "Filter Keyword: ";
    private const string ReporterRunLogCollectorKeywordsPrefix = "Collector Keywords: ";
    private const string ReporterRunLogSortModePrefix = "Sort Mode: ";
    private const string ReporterRunLogScannedCountPrefix = "扫描文件数: ";
    private const string ReporterRunLogMatchedCountPrefix = "匹配文件数: ";
    private const string ReporterRunStatusCompletedPrefix = "Reporter 原型执行完成：匹配 ";
    private const string ReporterRunStatusCompletedSuffix = " 项";
    private const string ReporterLoadBuildReportLogStart = "开始读取构建报告...";
    private const string ReporterLoadBuildReportUnavailable = "Reporter 报告读取失败：未找到构建结果。";
    private const string ReporterLoadBuildReportCompleted = "Reporter 构建报告读取完成。";
    private const string ReporterLoadBuildReportMissingManifest = "构建报告缺失 build_manifest.txt";
    private const string ReporterLoadBuildReportMissingVersion = "构建报告缺失 build_version.txt";
    private const string ReporterLoadBuildReportRuntimeVersionMissing = "Runtime 版本文件缺失";
    private const string ReporterLoadBuildReportRuntimeHashMissing = "Runtime 哈希文件缺失";
    private const string ReporterLoadBuildReportRuntimeManifestMissing = "Runtime 清单文件缺失";
    private const string ReporterLoadBuildReportRuntimeHashMismatch = "Runtime 哈希校验失败";
    private const string ReporterLoadBuildReportRuntimeVersionMismatch = "Runtime 版本不一致";
    private const string ReporterSummaryScanRootPrefix = "Scan Root: ";
    private const string ReporterSummaryGlobalRootPrefix = "Global Root: ";
    private const string ReporterSummaryPipelinePrefix = "Pipeline: ";
    private const string ReporterSummaryFilterKeywordPrefix = "Filter Keyword: ";
    private const string ReporterSummaryCollectorKeywordsPrefix = "Collector Keywords: ";
    private const string ReporterSummarySortModePrefix = "Sort Mode: ";
    private const string ReporterSummaryScannedFilesPrefix = "Scanned Files: ";
    private const string ReporterSummaryMatchedFilesPrefix = "Matched Files: ";
    private const string ReporterSummaryTopExtensionsPrefix = "Top Extensions:";
    private const string ReporterSummaryScanLimitHitPrefix = "Scan Limit Hit: ";
    private const string ReporterDefaultPipeline = "RawFileBuildPipeline";
    private const string ReporterDefaultSortMode = "Count Desc";
    private const string ReporterDefaultScanRoot = ProjectDisplayPrefix;
    private const int ReporterScanLimit = 10000;
    private const int ReporterSummaryTopExtensionCount = 10;
    private const string ReporterNoExtensionPlaceholder = "(no-ext)";
    private const string ReporterRunStatusScanRootUnavailable = "Reporter 执行失败：扫描目录不可用。";
    private const string CommonRunLogFailedPrefix = "执行失败：";
    private const string ReporterRunLogFailedPrefix = CommonRunLogFailedPrefix;
    private const string PluginLogTimeFormat = "HH:mm:ss";
    private const string ExportIsoDateTimeFormat = "O";
    private const string PluginLogEntryPrefix = "[";
    private const string PluginLogEntrySuffix = "] ";
    private const string PluginLogLineTerminator = "\n";
    private const string ReporterSummaryLineSeparator = PluginLogLineTerminator;
    private const string BuilderRunStatusOutputPathUnavailable = "Builder 执行失败：输出目录不可用。";
    private const string BuilderRunLogFailedPrefix = CommonRunLogFailedPrefix;
    private const string BuilderRunLogStart = "开始执行 Builder...";
    private const string BuilderRunLogPackageNamePrefix = "Package Name: ";
    private const string BuilderRunLogOutputPathPrefix = "Output Path: ";
    private const string BuilderRunLogGlobalOutputPrefix = "Global Output: ";
    private const string BuilderRunLogPipelinePrefix = "Pipeline: ";
    private const string BuilderRunLogScanRootPrefix = "Scan Root: ";
    private const string BuilderRunLogKeywordsPrefix = "Keywords: ";
    private const string BuilderRunLogCompleted = "构建执行完成。";
    private const string BuilderRunStatusCompletedPrefix = "Builder 构建完成：";
    private const string BuilderRunLogBuildFailedPrefix = "构建失败：";
    private const string BuilderRunStatusFailed = "Builder 构建失败。";
    private const string BuilderRunSummaryFormat = "Pipeline={0}, Version={1}, Scanned={2}, Files={3}, ScanLimitHit={4}, Output={5}";
    private const string BuilderStagePrepare = "Prepare";
    private const string BuilderStagePackage = "Package";
    private const string BuilderStageManifest = "Manifest";
    private const string BuilderStageRuntimeLink = "RuntimeLink";
    private const string BuilderStageVerify = "Verify";
    private const string BuilderStageStartPrefix = "阶段开始: ";
    private const string BuilderStageCompletedPrefix = "阶段完成: ";
    private const string BuilderVerifyErrorOutputRootMissing = "构建校验失败：输出目录不存在。";
    private const string BuilderVerifyErrorManifestMissing = "构建校验失败：清单文件不存在。";
    private const string BuilderVerifyErrorVersionFileMissing = "构建校验失败：版本文件不存在。";
    private const string BuilderVerifyErrorRuntimeVersionFileMissing = "构建校验失败：Runtime 版本文件不存在。";
    private const string BuilderVerifyErrorRuntimeManifestFileMissing = "构建校验失败：Runtime 清单文件不存在。";
    private const string BuilderVerifyErrorRuntimeHashFileMissing = "构建校验失败：Runtime 哈希文件不存在。";
    private const string BuilderVerifyErrorFileCountMismatch = "构建校验失败：复制文件数与收集文件数不一致。";
    private const string BuilderVerifyErrorFileMissingPrefix = "构建校验失败：缺少输出文件 ";
    private const string BuilderSyncReporterLogPrefix = "已同步 Reporter 参数：Scan Root=";
    private const string BuilderSyncReporterLogMid = ", Keywords=";
    private const string BuilderSyncReporterToReporterLog = "已从最近一次 Builder 同步 Scan Root 与 Filter Keyword。";
    private const string DebuggerStatusUnchecked = "状态: 未检查";
    private const string DebuggerPathUnchecked = "路径: 未检查";
    private const string DebuggerTimeScaleNumberFormat = "0.##";
    private const string DebuggerStatusSnapshotFormat = "状态: EditorHint={0}, FPS={1}, TimeScale={2}";
    private const string DebuggerPathSnapshotFormat = "路径: res:// => {0} | user:// => {1}";
    private const string DebuggerLogSnapshotRefreshed = "刷新运行时快照完成。";
    private const string DebuggerLogOsPrefix = "OS: ";
    private const string DebuggerLogEditorHintPrefix = "EditorHint: ";
    private const string DebuggerLogFpsPrefix = "FPS: ";
    private const string DebuggerLogTimeScalePrefix = "TimeScale: ";
    private const string DebuggerLogResExistsPrefix = "res:// exists: ";
    private const string DebuggerLogUserExistsPrefix = "user:// exists: ";
    private const string DebuggerStatusSnapshotRefreshed = "Debugger 快照已刷新。";
    private const string DebuggerPathCheckKeyRes = ProjectDisplayPrefix;
    private const string UserDataPrefix = "user://";
    private const string DebuggerPathCheckKeyUser = UserDataPrefix;
    private const string DebuggerPathCheckKeyBuilds = BuilderDefaultOutputPath;
    private const string DebuggerPathCheckKeyReports = ReporterExportOutputDirectory;
    private const string DebuggerPathCheckLogFormat = "{0} => {1} | exists={2}";
    private const string DebuggerStatusPathCheckFormat = "Debugger 路径诊断完成：{0}/{1} 通过";
    private const string DebuggerStatusRuntimeCommandCompleted = "Debugger 运行时指令执行完成。";
    private const string DebuggerStatusRuntimeCommandFailedPrefix = "Debugger 运行时指令执行失败：";
    private const string DebuggerRuntimeCommandDefault = "sample_once";
    private const string ButtonRunRuntimeCommand = "Run Runtime Command";
    private const string FieldLabelRuntimeCommand = "Runtime Command";
    private const string FieldLabelRuntimeParam = "Runtime Param";
    private const string DebuggerLogRuntimeCommandPrefix = "RuntimeCommand: ";
    private const string DebuggerLogRuntimeParamPrefix = "RuntimeParam: ";
    private const string DebuggerLogRuntimeResultPrefix = "RuntimeResult: ";
    private const string DebuggerLogRuntimeFramePrefix = "FrameCount: ";
    private const string DebuggerLogRuntimePackagePrefix = "Package: ";
    private const string DebuggerLogRuntimeProviderCountPrefix = "ProviderCount: ";
    private const string DebuggerLogRuntimeProviderPrefix = "Provider: ";
    private const string DebuggerLogRuntimeProviderDetailSeparator = " | ";
    private const string DebuggerLogRuntimeProviderRefPrefix = "RefCount=";
    private const string DebuggerLogRuntimeProviderStatusPrefix = "Status=";
    private const string DebuggerStatusRuntimeCommandInvalidPrefix = "Debugger 指令格式错误：";
    private const string DebuggerLogRuntimeProtocolSendBytesPrefix = "ProtocolSendBytes: ";
    private const string DebuggerLogRuntimeProtocolReceiveBytesPrefix = "ProtocolReceiveBytes: ";
    private const string ButtonValidateDiagnosticAlignment = "Validate Diagnostic Alignment";
    private const string DebuggerStatusAlignmentPassed = "Debugger 字段对齐校验通过。";
    private const string DebuggerStatusAlignmentFailedPrefix = "Debugger 字段对齐校验失败：";
    private const string DebuggerLogAlignmentStart = "开始执行字段对齐校验。";
    private const string DebuggerLogAlignmentPassed = "字段对齐校验通过。";
    private const string DebuggerLogAlignmentMissingPrefix = "字段缺失: ";
    private const string DebuggerLogAlignmentInvalidPrefix = "字段无效: ";
    private const string TabSwitchStatusPrefix = "已切换到 ";
    private const string TabSwitchStatusSuffix = " 模块。";
    private const string TabSwitchStatusUnavailable = "模块切换失败：插件面板未就绪。";
    private const string TabSwitchStatusOutOfRangePrefix = "模块切换失败：索引越界 ";
    private const string PackageNameEmptyError = "Package Name 不能为空。";
    private const string OutputPathEmptyError = "Output Path 不能为空。";
    private const string BuilderDefaultScanRoot = ProjectDisplayPrefix;
    private const string BuilderParamErrorPackageNameStatus = "Builder 参数错误：" + PackageNameEmptyError;
    private const string BuilderParamErrorPackageNameLog = CommonRunLogFailedPrefix + PackageNameEmptyError;
    private const string BuilderParamErrorOutputPathStatus = "Builder 参数错误：" + OutputPathEmptyError;
    private const string BuilderParamErrorOutputPathLog = CommonRunLogFailedPrefix + OutputPathEmptyError;
    private const string BuilderBuildTimestampFormat = "yyyyMMdd_HHmmss";
    private const string BuilderPackageVersionFormat = "yyyyMMddHHmmss";
    private const string BuilderFilesDirectoryName = "files";
    private const string BuilderManifestFileName = "build_manifest.txt";
    private const string BuilderVersionFileName = "build_version.txt";
    private const string ReportFieldPackageRoot = "PackageRoot";
    private const string ReportFieldManifestAvailable = "BuildManifestAvailable";
    private const string ReportFieldVersionAvailable = "BuildVersionAvailable";
    private const string ReportFieldRuntimeVersionAvailable = "RuntimeVersionAvailable";
    private const string ReportFieldRuntimeHashAvailable = "RuntimeHashAvailable";
    private const string ReportFieldRuntimeManifestAvailable = "RuntimeManifestAvailable";
    private const string ReportFieldRuntimeHashMatch = "RuntimeHashMatch";
    private const string ReportFieldRuntimeVersionMatch = "RuntimeVersionMatch";
    private const string ReportFieldRuntimeHashValue = "RuntimeHashValue";
    private const string ReportFieldRuntimeActualHashValue = "RuntimeActualHashValue";
    private const string BuilderNoBuildResourceError = "未找到可构建资源。";
    private const string ExtensionSummarySortModeAsc = "Extension Asc";
    private const string ExtensionSummaryEmpty = "(empty)";
    private const string ExtensionSummaryLineFormat = "{0}. {1} : {2}";
    private const string PluginRootName = "YooAsset";
    private const string PluginTitleText = "YooAsset Editor (Godot)";
    private const string PluginLoadedStatus = "YooAsset 插件已加载，当前为模块原型阶段。";
    private const string HomePageUrl = "https://www.yooasset.com/";
    private const string BuilderModuleName = "Builder";
    private const string CollectorModuleName = "Collector";
    private const string ReporterModuleName = "Reporter";
    private const string DebuggerModuleName = "Debugger";
    private const string BuilderReadyLog = "Builder 已就绪。";
    private const string CollectorReadyLog = "Collector 原型已就绪。";
    private const string ReporterReadyLog = "Reporter 原型已就绪。";
    private const string DebuggerReadyLog = "Debugger 原型已就绪。";
    private const string ModulePageTipText = "当前版本仅提供入口与结构，功能将在后续阶段逐步落地。";
    private const string ReporterOpenOutputUnavailable = "打开失败：最近一次构建输出目录不可用。";
    private const string ReporterOpenOutputLogPrefix = "已打开构建输出目录：";
    private const string ReporterOpenOutputSuccess = "已打开最近一次构建输出目录。";
    private const string ReporterOpenManifestUnavailable = "打开失败：最近一次构建清单文件不可用。";
    private const string ReporterOpenManifestLogPrefix = "已打开构建清单文件：";
    private const string ReporterOpenManifestSuccess = "已打开最近一次构建清单文件。";
    private const string ModuleTitleFormat = "[{0}]";
    private const string BuilderSubtitleText = "构建流程：参数输入、资源收集、产物输出、清单生成。";
    private const string CollectorSubtitleText = "最小采集流程原型：路径输入、规则过滤、结果预览。";
    private const string ReporterSubtitleText = "最小报告原型：扫描汇总、筛选统计、导出占位。";
    private const string DebuggerSubtitleText = "最小调试原型：运行状态检查、路径诊断、日志输出。";
    private const string FieldLabelPackageName = "Package Name";
    private const string FieldLabelOutputPath = "Output Path";
    private const string FieldLabelPipeline = "Pipeline";
    private const string FieldLabelScanRoot = "Scan Root";
    private const string FieldLabelKeywords = "Keywords (; 分隔)";
    private const string FieldLabelFilterKeyword = "Filter Keyword";
    private const string FieldLabelSortMode = "Sort Mode";
    private const string BuilderDefaultPackageName = "DefaultPackage";
    private const string BuilderDefaultOutputPath = "user://yooasset_builds";
    private const string BuilderPipelineScriptable = "ScriptableBuildPipeline";
    private const string CollectorDefaultKeywords = ".png;.tres;.tscn";
    private const string ButtonRunBuilderBuild = "Run Builder Build";
    private const string ButtonRunCollectorPrototype = "Run Collector Prototype";
    private const string ButtonRunReporterPrototype = "Run Reporter Prototype";
    private const string ButtonLoadBuildReport = "Load Build Report";
    private const string ButtonExportSummaryPlaceholder = "Export Summary Placeholder";
    private const string ButtonOpenLastBuildOutput = "Open Last Build Output";
    private const string ButtonOpenLastBuildManifest = "Open Last Build Manifest";
    private const string ButtonRefreshRuntimeSnapshot = "Refresh Runtime Snapshot";
    private const string ButtonRunPathDiagnostics = "Run Path Diagnostics";
    private const string SourceFilterIgnoreDirectory = "/.godot/";
    private const string SourceFilterIgnoreImportExtension = ".import";
    private const string SourceFilterIgnoreUidExtension = ".uid";
    private const string SourceFilterIgnoreScriptExtension = ".cs";
    private const string SourceFilterSceneExtension = ".tscn";
    private const string SourceFilterResourceExtension = ".tres";
    private const string SourceFilterBinaryResourceExtension = ".res";
    private const string SourceFilterImagePngExtension = ".png";
    private const string SourceFilterImageJpgExtension = ".jpg";
    private const string SourceFilterImageJpegExtension = ".jpeg";
    private const string SourceFilterImageWebpExtension = ".webp";
    private const string SourceFilterAudioMp3Extension = ".mp3";
    private const string SourceFilterAudioWavExtension = ".wav";
    private const string SourceFilterAudioOggExtension = ".ogg";
    private const string SourceFilterModelGlbExtension = ".glb";
    private const string SourceFilterModelGltfExtension = ".gltf";
    private const string SourceFilterJsonExtension = ".json";
    private const string SourceFilterTextExtension = ".txt";
    private const string SourceFilterBytesExtension = ".bytes";
    private const string PipelineKeywordRawFile = ".png;.jpg;.jpeg;.webp;.mp3;.wav;.ogg;.glb;.gltf;.json;.txt;.bytes;.tscn;.tres;.res";
    private const string PipelineKeywordSceneOnly = ".tscn;.tres;.res";
    private const char KeywordSeparator = ';';
    private const string FileSearchPatternAll = "*";
    private const string CollectorParamErrorScanRootStatus = "Collector 参数错误：" + ScanRootEmptyError;
    private const string CollectorParamErrorScanRootLog = CommonRunLogFailedPrefix + ScanRootEmptyError;
    private const string CollectorRunStatusScanRootUnavailable = "Collector 执行失败：扫描目录不可用。";
    private const int CollectorPreviewLimit = 20;
    private const int CollectorScanLimit = 5000;
    private const string CollectorRunLogStart = "开始执行 Collector 原型...";
    private const string CollectorRunLogScanRootPrefix = "Scan Root: ";
    private const string CollectorRunLogGlobalRootPrefix = "Global Root: ";
    private const string CollectorRunLogPipelinePrefix = "Pipeline: ";
    private const string CollectorRunLogKeywordsPrefix = "Keywords: ";
    private const string CollectorRunLogScannedCountPrefix = "扫描文件数: ";
    private const string CollectorRunLogMatchedCountPrefix = "匹配结果数: ";
    private const string CollectorRunLogPreviewLimitPrefix = "预览上限: ";
    private const string CollectorRunLogScanLimitHitPrefix = "已触发扫描上限: ";
    private const string CollectorRunStatusCompletedPrefix = "Collector 原型执行完成：匹配 ";
    private const string CollectorRunStatusCompletedSuffix = " 项";
    private const string ScanRootEmptyError = "Scan Root 不能为空。";
    private const string ScanRootNotExistsErrorPrefix = "目录不存在 ";
    private const string ManifestFieldPackageName = "PackageName";
    private const string ManifestFieldPipeline = "Pipeline";
    private const string ManifestFieldBuildVersion = "BuildVersion";
    private const string ManifestFieldBuildTime = "BuildTime";
    private const string ManifestFieldSourceRoot = "SourceRoot";
    private const string ManifestFieldOutputFilesRoot = "OutputFilesRoot";
    private const string ManifestFieldFileCount = "FileCount";
    private const string ManifestSectionFiles = "Files:";
    private const string ProjectDisplayPrefix = "res://";
    private const float UiMinWidthAuto = 0f;
    private const float BuilderLogMinHeight = 180f;
    private const float CollectorViewMinHeight = 140f;
    private const float ReporterSummaryMinHeight = 150f;
    private const float ReporterLogMinHeight = 120f;
    private const float DebuggerLogMinHeight = 220f;
    private const DockSlot PluginDockSlot = DockSlot.LeftUl;
    private static readonly HashSet<string> RawFilePipelineExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        SourceFilterSceneExtension,
        SourceFilterResourceExtension,
        SourceFilterBinaryResourceExtension,
        SourceFilterImagePngExtension,
        SourceFilterImageJpgExtension,
        SourceFilterImageJpegExtension,
        SourceFilterImageWebpExtension,
        SourceFilterAudioMp3Extension,
        SourceFilterAudioWavExtension,
        SourceFilterAudioOggExtension,
        SourceFilterModelGlbExtension,
        SourceFilterModelGltfExtension,
        SourceFilterJsonExtension,
        SourceFilterTextExtension,
        SourceFilterBytesExtension
    };
    private static readonly HashSet<string> ScenePipelineExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        SourceFilterSceneExtension,
        SourceFilterResourceExtension,
        SourceFilterBinaryResourceExtension
    };

    private Control _dock;
    private TabContainer _tabContainer;
    private RichTextLabel _statusView;
    private LineEdit _builderPackageNameInput;
    private LineEdit _builderOutputPathInput;
    private OptionButton _builderPipelineOptions;
    private RichTextLabel _builderLogView;
    private LineEdit _collectorScanRootInput;
    private LineEdit _collectorKeywordInput;
    private RichTextLabel _collectorPreviewView;
    private RichTextLabel _collectorLogView;
    private LineEdit _reporterScanRootInput;
    private LineEdit _reporterKeywordInput;
    private OptionButton _reporterSortOptions;
    private RichTextLabel _reporterSummaryView;
    private RichTextLabel _reporterLogView;
    private string _reporterLastSummary = string.Empty;
    private Dictionary<string, string> _reporterSummaryFields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    private string _lastBuildPackageName = string.Empty;
    private string _lastBuildPipeline = string.Empty;
    private string _lastBuildOutputDirectory = string.Empty;
    private string _lastBuildManifestPath = string.Empty;
    private int _lastBuildFileCount = 0;
    private int _lastBuildScannedCount = 0;
    private bool _lastBuildScanLimitHit = false;
    private DateTime _lastBuildTime = DateTime.MinValue;
    private Label _debuggerStatusLabel;
    private Label _debuggerPathLabel;
    private LineEdit _debuggerCommandInput;
    private LineEdit _debuggerCommandParamInput;
    private RichTextLabel _debuggerLogView;
    private DebugReport _lastRuntimeDebugReport;
    private bool _isLifecycleMounted;
    private bool _toolMenusRegistered;
    private int _lastOpenedTabIndex = DefaultTabIndex;
    private IEditorPlatformBridge _editorPlatformBridge;

    /// <summary>
    /// 插件进入编辑器树时挂载菜单与面板
    /// </summary>
    public override void _EnterTree()
    {
        EnsureEditorBridge();
        if (_isLifecycleMounted)
        {
            EnsureLifecycleState();
            return;
        }

        RegisterToolMenus();
        MountDock();
        _isLifecycleMounted = true;
        EnsureLifecycleState();
    }

    /// <summary>
    /// 插件退出编辑器树时释放菜单与面板
    /// </summary>
    public override void _ExitTree()
    {
        if (_isLifecycleMounted == false)
        {
            ForceCleanupOrphanState();
            return;
        }

        UnmountDock();
        UnregisterToolMenus();
        _isLifecycleMounted = false;
    }

    /// <summary>
    /// 确保编辑器桥接器已初始化
    /// </summary>
    private void EnsureEditorBridge()
    {
        _editorPlatformBridge ??= new GodotEditorPlatformBridge();
    }

    /// <summary>
    /// 校正插件生命周期状态，避免启停后残留不一致
    /// </summary>
    private void EnsureLifecycleState()
    {
        if (_toolMenusRegistered == false)
        {
            RegisterToolMenus();
        }

        if (_dock == null || _tabContainer == null)
        {
            MountDock();
        }
    }

    /// <summary>
    /// 清理异常残留状态，确保退出阶段幂等
    /// </summary>
    private void ForceCleanupOrphanState()
    {
        if (_dock != null || _tabContainer != null || _statusView != null)
        {
            UnmountDock();
        }

        if (_toolMenusRegistered)
        {
            UnregisterToolMenus();
        }
    }

    /// <summary>
    /// 注册工具菜单入口
    /// </summary>
    private void RegisterToolMenus()
    {
        if (_toolMenusRegistered)
        {
            return;
        }

        RegisterToolMenuItem(HomePageMenu, OpenHomePage);
        RegisterToolMenuItem(BuilderMenu, OpenBuilder);
        RegisterToolMenuItem(CollectorMenu, OpenCollector);
        RegisterToolMenuItem(ReporterMenu, OpenReporter);
        RegisterToolMenuItem(DebuggerMenu, OpenDebugger);
        _toolMenusRegistered = true;
    }

    /// <summary>
    /// 注销工具菜单入口
    /// </summary>
    private void UnregisterToolMenus()
    {
        if (_toolMenusRegistered == false)
        {
            return;
        }

        RemoveToolMenuItem(HomePageMenu);
        RemoveToolMenuItem(BuilderMenu);
        RemoveToolMenuItem(CollectorMenu);
        RemoveToolMenuItem(ReporterMenu);
        RemoveToolMenuItem(DebuggerMenu);
        _toolMenusRegistered = false;
    }

    private void RegisterToolMenuItem(string menuPath, Action callback)
    {
        RemoveToolMenuItem(menuPath);
        AddToolMenuItem(menuPath, Callable.From(callback));
    }

    /// <summary>
    /// 创建并挂载主面板
    /// </summary>
    private void MountDock()
    {
        EnsureEditorBridge();
        if (_dock != null)
        {
            return;
        }

        var root = new VBoxContainer();
        root.Name = PluginRootName;
        root.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        root.SizeFlagsVertical = Control.SizeFlags.ExpandFill;

        var title = new Label();
        title.Text = PluginTitleText;
        root.AddChild(title);

        var actions = new HBoxContainer();
        actions.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        root.AddChild(actions);

        actions.AddChild(CreateActionButton(BuilderModuleName, OpenBuilder));
        actions.AddChild(CreateActionButton(CollectorModuleName, OpenCollector));
        actions.AddChild(CreateActionButton(ReporterModuleName, OpenReporter));
        actions.AddChild(CreateActionButton(DebuggerModuleName, OpenDebugger));

        _tabContainer = new TabContainer();
        _tabContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _tabContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        root.AddChild(_tabContainer);

        _tabContainer.AddChild(CreateBuilderPage());
        _tabContainer.AddChild(CreateCollectorPage());
        _tabContainer.AddChild(CreateReporterPage());
        _tabContainer.AddChild(CreateDebuggerPage());
        if (_lastOpenedTabIndex >= 0 && _lastOpenedTabIndex < _tabContainer.GetTabCount())
        {
            _tabContainer.CurrentTab = _lastOpenedTabIndex;
        }

        _statusView = new RichTextLabel();
        _statusView.FitContent = true;
        _statusView.ScrollActive = false;
        _statusView.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        root.AddChild(_statusView);
        SetStatus(PluginLoadedStatus);

        _dock = root;
        AddControlToDock(PluginDockSlot, _dock);
    }

    /// <summary>
    /// 卸载并释放主面板
    /// </summary>
    private void UnmountDock()
    {
        if (_tabContainer != null && _tabContainer.GetTabCount() > 0)
        {
            _lastOpenedTabIndex = _tabContainer.CurrentTab;
        }

        if (_dock != null)
        {
            RemoveControlFromDocks(_dock);
            _dock.QueueFree();
            _dock = null;
        }
        _tabContainer = null;
        _statusView = null;
        _builderPackageNameInput = null;
        _builderOutputPathInput = null;
        _builderPipelineOptions = null;
        _builderLogView = null;
        _collectorScanRootInput = null;
        _collectorKeywordInput = null;
        _collectorPreviewView = null;
        _collectorLogView = null;
        _reporterScanRootInput = null;
        _reporterKeywordInput = null;
        _reporterSortOptions = null;
        _reporterSummaryView = null;
        _reporterLogView = null;
        _debuggerStatusLabel = null;
        _debuggerPathLabel = null;
        _debuggerLogView = null;
    }

    private void OpenHomePage()
    {
        EnsureEditorBridge();
        _editorPlatformBridge.OpenExternalPath(HomePageUrl);
    }

    private void OpenBuilder()
    {
        OpenModule(BuilderTabIndex, BuilderModuleName);
    }

    private void OpenCollector()
    {
        OpenModule(CollectorTabIndex, CollectorModuleName);
    }

    private void OpenReporter()
    {
        OpenModule(ReporterTabIndex, ReporterModuleName);
    }

    private void OpenDebugger()
    {
        OpenModule(DebuggerTabIndex, DebuggerModuleName);
    }

    private void OpenModule(int tabIndex, string moduleName)
    {
        EnsureDockReady();
        SwitchTab(tabIndex, moduleName);
    }

    private void EnsureDockReady()
    {
        EnsureEditorBridge();
        if (_dock == null || _tabContainer == null)
        {
            MountDock();
        }
    }

    private Button CreateActionButton(string text, Action onPressed)
    {
        var button = new Button();
        button.Text = text;
        button.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        button.Pressed += onPressed;
        return button;
    }

    private Control CreateBuilderPage()
    {
        var page = new VBoxContainer();
        page.Name = BuilderModuleName;
        page.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        page.SizeFlagsVertical = Control.SizeFlags.ExpandFill;

        var titleLabel = new Label();
        titleLabel.Text = string.Format(ModuleTitleFormat, BuilderModuleName);
        page.AddChild(titleLabel);

        var subtitleLabel = new Label();
        subtitleLabel.Text = BuilderSubtitleText;
        subtitleLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        page.AddChild(subtitleLabel);

        page.AddChild(CreateFieldLabel(FieldLabelPackageName));
        _builderPackageNameInput = new LineEdit();
        _builderPackageNameInput.Text = BuilderDefaultPackageName;
        page.AddChild(_builderPackageNameInput);

        page.AddChild(CreateFieldLabel(FieldLabelOutputPath));
        _builderOutputPathInput = new LineEdit();
        _builderOutputPathInput.Text = BuilderDefaultOutputPath;
        page.AddChild(_builderOutputPathInput);

        page.AddChild(CreateFieldLabel(FieldLabelPipeline));
        _builderPipelineOptions = new OptionButton();
        _builderPipelineOptions.AddItem(ReporterDefaultPipeline);
        _builderPipelineOptions.AddItem(BuilderPipelineScriptable);
        page.AddChild(_builderPipelineOptions);

        var runButton = new Button();
        runButton.Text = ButtonRunBuilderBuild;
        runButton.Pressed += RunBuilderPrototype;
        page.AddChild(runButton);

        _builderLogView = new RichTextLabel();
        _builderLogView.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _builderLogView.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        _builderLogView.CustomMinimumSize = new Vector2(UiMinWidthAuto, BuilderLogMinHeight);
        page.AddChild(_builderLogView);

        AppendBuilderLog(BuilderReadyLog);
        return page;
    }

    private static Label CreateFieldLabel(string text)
    {
        var label = new Label();
        label.Text = text;
        return label;
    }

    private Control CreateCollectorPage()
    {
        var page = new VBoxContainer();
        page.Name = CollectorModuleName;
        page.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        page.SizeFlagsVertical = Control.SizeFlags.ExpandFill;

        var titleLabel = new Label();
        titleLabel.Text = string.Format(ModuleTitleFormat, CollectorModuleName);
        page.AddChild(titleLabel);

        var subtitleLabel = new Label();
        subtitleLabel.Text = CollectorSubtitleText;
        subtitleLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        page.AddChild(subtitleLabel);

        page.AddChild(CreateFieldLabel(FieldLabelScanRoot));
        _collectorScanRootInput = new LineEdit();
        _collectorScanRootInput.Text = ReporterDefaultScanRoot;
        page.AddChild(_collectorScanRootInput);

        page.AddChild(CreateFieldLabel(FieldLabelKeywords));
        _collectorKeywordInput = new LineEdit();
        _collectorKeywordInput.Text = CollectorDefaultKeywords;
        page.AddChild(_collectorKeywordInput);

        var runButton = new Button();
        runButton.Text = ButtonRunCollectorPrototype;
        runButton.Pressed += RunCollectorPrototype;
        page.AddChild(runButton);

        _collectorPreviewView = new RichTextLabel();
        _collectorPreviewView.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _collectorPreviewView.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        _collectorPreviewView.CustomMinimumSize = new Vector2(UiMinWidthAuto, CollectorViewMinHeight);
        page.AddChild(_collectorPreviewView);

        _collectorLogView = new RichTextLabel();
        _collectorLogView.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _collectorLogView.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        _collectorLogView.CustomMinimumSize = new Vector2(UiMinWidthAuto, CollectorViewMinHeight);
        page.AddChild(_collectorLogView);

        AppendCollectorLog(CollectorReadyLog);
        return page;
    }

    private Control CreateReporterPage()
    {
        var page = new VBoxContainer();
        page.Name = ReporterModuleName;
        page.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        page.SizeFlagsVertical = Control.SizeFlags.ExpandFill;

        var titleLabel = new Label();
        titleLabel.Text = string.Format(ModuleTitleFormat, ReporterModuleName);
        page.AddChild(titleLabel);

        var subtitleLabel = new Label();
        subtitleLabel.Text = ReporterSubtitleText;
        subtitleLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        page.AddChild(subtitleLabel);

        page.AddChild(CreateFieldLabel(FieldLabelScanRoot));
        _reporterScanRootInput = new LineEdit();
        _reporterScanRootInput.Text = ReporterDefaultScanRoot;
        page.AddChild(_reporterScanRootInput);

        page.AddChild(CreateFieldLabel(FieldLabelFilterKeyword));
        _reporterKeywordInput = new LineEdit();
        _reporterKeywordInput.Text = string.Empty;
        page.AddChild(_reporterKeywordInput);

        page.AddChild(CreateFieldLabel(FieldLabelSortMode));
        _reporterSortOptions = new OptionButton();
        _reporterSortOptions.AddItem(ReporterDefaultSortMode);
        _reporterSortOptions.AddItem(ExtensionSummarySortModeAsc);
        page.AddChild(_reporterSortOptions);

        var actions = new HBoxContainer();
        actions.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        page.AddChild(actions);

        var runButton = new Button();
        runButton.Text = ButtonRunReporterPrototype;
        runButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        runButton.Pressed += RunReporterPrototype;
        actions.AddChild(runButton);

        var loadBuildReportButton = new Button();
        loadBuildReportButton.Text = ButtonLoadBuildReport;
        loadBuildReportButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        loadBuildReportButton.Pressed += LoadLatestBuildReport;
        actions.AddChild(loadBuildReportButton);

        var exportButton = new Button();
        exportButton.Text = ButtonExportSummaryPlaceholder;
        exportButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        exportButton.Pressed += ExportReporterPlaceholder;
        actions.AddChild(exportButton);

        var quickActions = new HBoxContainer();
        quickActions.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        page.AddChild(quickActions);

        var openOutputButton = new Button();
        openOutputButton.Text = ButtonOpenLastBuildOutput;
        openOutputButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        openOutputButton.Pressed += OpenLastBuildOutputDirectory;
        quickActions.AddChild(openOutputButton);

        var openManifestButton = new Button();
        openManifestButton.Text = ButtonOpenLastBuildManifest;
        openManifestButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        openManifestButton.Pressed += OpenLastBuildManifestFile;
        quickActions.AddChild(openManifestButton);

        _reporterSummaryView = new RichTextLabel();
        _reporterSummaryView.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _reporterSummaryView.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        _reporterSummaryView.CustomMinimumSize = new Vector2(UiMinWidthAuto, ReporterSummaryMinHeight);
        page.AddChild(_reporterSummaryView);

        _reporterLogView = new RichTextLabel();
        _reporterLogView.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _reporterLogView.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        _reporterLogView.CustomMinimumSize = new Vector2(UiMinWidthAuto, ReporterLogMinHeight);
        page.AddChild(_reporterLogView);

        AppendReporterLog(ReporterReadyLog);
        return page;
    }

    private Control CreateDebuggerPage()
    {
        var page = new VBoxContainer();
        page.Name = DebuggerModuleName;
        page.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        page.SizeFlagsVertical = Control.SizeFlags.ExpandFill;

        var titleLabel = new Label();
        titleLabel.Text = string.Format(ModuleTitleFormat, DebuggerModuleName);
        page.AddChild(titleLabel);

        var subtitleLabel = new Label();
        subtitleLabel.Text = DebuggerSubtitleText;
        subtitleLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        page.AddChild(subtitleLabel);

        _debuggerStatusLabel = new Label();
        _debuggerStatusLabel.Text = DebuggerStatusUnchecked;
        page.AddChild(_debuggerStatusLabel);

        _debuggerPathLabel = new Label();
        _debuggerPathLabel.Text = DebuggerPathUnchecked;
        _debuggerPathLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        page.AddChild(_debuggerPathLabel);

        var actions = new HBoxContainer();
        actions.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        page.AddChild(actions);

        var refreshButton = new Button();
        refreshButton.Text = ButtonRefreshRuntimeSnapshot;
        refreshButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        refreshButton.Pressed += RefreshDebuggerSnapshot;
        actions.AddChild(refreshButton);

        var checkButton = new Button();
        checkButton.Text = ButtonRunPathDiagnostics;
        checkButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        checkButton.Pressed += RunDebuggerPathCheck;
        actions.AddChild(checkButton);

        var alignmentButton = new Button();
        alignmentButton.Text = ButtonValidateDiagnosticAlignment;
        alignmentButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        alignmentButton.Pressed += RunDiagnosticFieldAlignmentCheck;
        actions.AddChild(alignmentButton);

        var runtimeCommandFields = new HBoxContainer();
        runtimeCommandFields.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        page.AddChild(runtimeCommandFields);

        var commandLabel = new Label();
        commandLabel.Text = FieldLabelRuntimeCommand;
        runtimeCommandFields.AddChild(commandLabel);

        _debuggerCommandInput = new LineEdit();
        _debuggerCommandInput.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _debuggerCommandInput.PlaceholderText = DebuggerRuntimeCommandDefault;
        _debuggerCommandInput.Text = DebuggerRuntimeCommandDefault;
        runtimeCommandFields.AddChild(_debuggerCommandInput);

        var paramLabel = new Label();
        paramLabel.Text = FieldLabelRuntimeParam;
        runtimeCommandFields.AddChild(paramLabel);

        _debuggerCommandParamInput = new LineEdit();
        _debuggerCommandParamInput.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        runtimeCommandFields.AddChild(_debuggerCommandParamInput);

        var runtimeCommandButton = new Button();
        runtimeCommandButton.Text = ButtonRunRuntimeCommand;
        runtimeCommandButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        runtimeCommandButton.Pressed += RunRuntimeDebugCommand;
        page.AddChild(runtimeCommandButton);

        _debuggerLogView = new RichTextLabel();
        _debuggerLogView.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _debuggerLogView.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        _debuggerLogView.CustomMinimumSize = new Vector2(UiMinWidthAuto, DebuggerLogMinHeight);
        page.AddChild(_debuggerLogView);

        AppendDebuggerLog(DebuggerReadyLog);
        RefreshDebuggerSnapshot();
        return page;
    }

    private Control CreateModulePage(string title, string subtitle)
    {
        var page = new VBoxContainer();
        page.Name = title;
        page.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        page.SizeFlagsVertical = Control.SizeFlags.ExpandFill;

        var titleLabel = new Label();
        titleLabel.Text = string.Format(ModuleTitleFormat, title);
        page.AddChild(titleLabel);

        var subtitleLabel = new Label();
        subtitleLabel.Text = subtitle;
        subtitleLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        page.AddChild(subtitleLabel);

        var tipLabel = new Label();
        tipLabel.Text = ModulePageTipText;
        tipLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        page.AddChild(tipLabel);

        return page;
    }

    private void RunBuilderPrototype()
    {
        var packageName = _builderPackageNameInput?.Text?.Trim() ?? string.Empty;
        var outputPath = _builderOutputPathInput?.Text?.Trim() ?? string.Empty;
        var pipeline = _builderPipelineOptions == null ? string.Empty : _builderPipelineOptions.GetItemText(_builderPipelineOptions.Selected);
        var collectorScanRoot = _collectorScanRootInput?.Text?.Trim();
        if (string.IsNullOrEmpty(collectorScanRoot))
        {
            collectorScanRoot = BuilderDefaultScanRoot;
        }

        var collectorKeywords = _collectorKeywordInput?.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(collectorKeywords))
        {
            collectorKeywords = GetDefaultPipelineKeywords(pipeline);
        }

        if (string.IsNullOrEmpty(packageName))
        {
            SetStatus(BuilderParamErrorPackageNameStatus);
            AppendBuilderLog(BuilderParamErrorPackageNameLog);
            return;
        }

        if (string.IsNullOrEmpty(outputPath))
        {
            SetStatus(BuilderParamErrorOutputPathStatus);
            AppendBuilderLog(BuilderParamErrorOutputPathLog);
            return;
        }

        string globalOutputPath;
        try
        {
            globalOutputPath = _editorPlatformBridge.GlobalizePath(outputPath);
            Directory.CreateDirectory(globalOutputPath);
        }
        catch (Exception ex)
        {
            SetStatus(BuilderRunStatusOutputPathUnavailable);
            AppendBuilderLog($"{BuilderRunLogFailedPrefix}{ex.Message}");
            return;
        }

        AppendBuilderLog(BuilderRunLogStart);
        AppendBuilderLog($"{BuilderRunLogPackageNamePrefix}{packageName}");
        AppendBuilderLog($"{BuilderRunLogOutputPathPrefix}{outputPath}");
        AppendBuilderLog($"{BuilderRunLogGlobalOutputPrefix}{globalOutputPath}");
        AppendBuilderLog($"{BuilderRunLogPipelinePrefix}{pipeline}");
        AppendBuilderLog($"{BuilderRunLogScanRootPrefix}{collectorScanRoot}");
        AppendBuilderLog($"{BuilderRunLogKeywordsPrefix}{collectorKeywords}");

        try
        {
            var buildResult = ExecuteBuilderBuild(packageName, globalOutputPath, pipeline, collectorScanRoot, collectorKeywords);
            _lastBuildPackageName = buildResult.PackageName;
            _lastBuildPipeline = buildResult.Pipeline;
            _lastBuildOutputDirectory = buildResult.OutputDirectory;
            _lastBuildManifestPath = buildResult.ManifestPath;
            _lastBuildFileCount = buildResult.FileCount;
            _lastBuildScannedCount = buildResult.ScannedCount;
            _lastBuildScanLimitHit = buildResult.ScanLimitHit;
            _lastBuildTime = buildResult.BuildTime;
            SyncReporterInputsFromBuilder(collectorScanRoot, collectorKeywords);
            AppendBuilderLog(BuilderRunLogCompleted);
            AppendBuilderLog(buildResult.Summary);
            SetStatus($"{BuilderRunStatusCompletedPrefix}{packageName}");
        }
        catch (Exception ex)
        {
            AppendBuilderLog($"{BuilderRunLogBuildFailedPrefix}{ex.Message}");
            SetStatus(BuilderRunStatusFailed);
        }
    }

    private void AppendBuilderLog(string message)
    {
        AppendLogEntry(_builderLogView, message);
    }

    private void SyncReporterInputsFromBuilder(string scanRoot, string keywordsText)
    {
        if (_reporterScanRootInput != null)
        {
            _reporterScanRootInput.Text = scanRoot;
        }

        if (_reporterKeywordInput != null)
        {
            _reporterKeywordInput.Text = keywordsText;
        }

        AppendBuilderLog($"{BuilderSyncReporterLogPrefix}{scanRoot}{BuilderSyncReporterLogMid}{keywordsText}");
        AppendReporterLog(BuilderSyncReporterToReporterLog);
    }

    private BuildExecutionResult ExecuteBuilderBuild(string packageName, string globalOutputPath, string pipeline, string scanRoot, string keywordsText)
    {
        var context = new BuildOrchestrationContext
        {
            PackageName = packageName,
            GlobalOutputPath = globalOutputPath,
            Pipeline = pipeline,
            ScanRoot = scanRoot,
            KeywordsText = keywordsText
        };

        RunPrepareBuildStage(context);
        RunPackageBuildStage(context);
        RunManifestBuildStage(context);
        RunRuntimeLinkBuildStage(context);
        RunVerifyBuildStage(context);
        return new BuildExecutionResult
        {
            PackageName = context.PackageName,
            Pipeline = context.Pipeline,
            PackageVersion = context.PackageVersion,
            OutputDirectory = context.PackageRoot,
            ManifestPath = context.ManifestPath,
            FileCount = context.CopiedCount,
            ScannedCount = context.ScannedCount,
            ScanLimitHit = context.ScanLimitHit,
            BuildTime = context.BuildTime,
            Summary = string.Format(BuilderRunSummaryFormat, context.Pipeline, context.PackageVersion, context.ScannedCount, context.CopiedCount, context.ScanLimitHit, context.PackageRoot)
        };
    }

    private void RunPrepareBuildStage(BuildOrchestrationContext context)
    {
        AppendBuilderLog($"{BuilderStageStartPrefix}{BuilderStagePrepare}");
        if (!TryCollectFiles(context.ScanRoot, context.KeywordsText, context.Pipeline, BuilderScanLimit, out var sourceRoot, out var collectedFiles, out var scannedCount, out var scanLimitHit, out var errorMessage))
        {
            throw new InvalidOperationException(errorMessage);
        }

        if (collectedFiles.Count == 0)
        {
            throw new InvalidOperationException(BuilderNoBuildResourceError);
        }

        context.SourceRoot = sourceRoot;
        context.CollectedFiles = collectedFiles;
        context.ScannedCount = scannedCount;
        context.ScanLimitHit = scanLimitHit;
        context.BuildTime = DateTime.Now;
        context.BuildTimestamp = context.BuildTime.ToString(BuilderBuildTimestampFormat);
        context.PackageVersion = context.BuildTime.ToString(BuilderPackageVersionFormat);
        context.PackageRoot = Path.Combine(context.GlobalOutputPath, context.PackageName, context.PackageVersion);
        context.FilesRoot = Path.Combine(context.PackageRoot, BuilderFilesDirectoryName);
        Directory.CreateDirectory(context.FilesRoot);
        AppendBuilderLog($"{BuilderStageCompletedPrefix}{BuilderStagePrepare}");
    }

    private void RunPackageBuildStage(BuildOrchestrationContext context)
    {
        AppendBuilderLog($"{BuilderStageStartPrefix}{BuilderStagePackage}");
        context.CopiedCount = 0;
        context.DestinationFiles.Clear();
        foreach (var sourceFile in context.CollectedFiles)
        {
            var relativePath = Path.GetRelativePath(context.SourceRoot, sourceFile);
            var destinationPath = Path.Combine(context.FilesRoot, relativePath);
            var destinationDir = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(destinationDir))
            {
                Directory.CreateDirectory(destinationDir);
            }

            File.Copy(sourceFile, destinationPath, true);
            context.DestinationFiles.Add(destinationPath);
            context.CopiedCount++;
        }

        AppendBuilderLog($"{BuilderStageCompletedPrefix}{BuilderStagePackage}");
    }

    private void RunManifestBuildStage(BuildOrchestrationContext context)
    {
        AppendBuilderLog($"{BuilderStageStartPrefix}{BuilderStageManifest}");
        context.ManifestPath = Path.Combine(context.PackageRoot, BuilderManifestFileName);
        context.VersionFilePath = Path.Combine(context.PackageRoot, BuilderVersionFileName);
        File.WriteAllText(context.ManifestPath, BuildManifestText(context.PackageName, context.Pipeline, context.PackageVersion, context.SourceRoot, context.FilesRoot, context.CollectedFiles, context.BuildTime), Encoding.UTF8);
        File.WriteAllText(context.VersionFilePath, BuildVersionText(context), Encoding.UTF8);
        AppendBuilderLog($"{BuilderStageCompletedPrefix}{BuilderStageManifest}");
    }

    private void RunRuntimeLinkBuildStage(BuildOrchestrationContext context)
    {
        AppendBuilderLog($"{BuilderStageStartPrefix}{BuilderStageRuntimeLink}");
        var runtimeManifest = new PackageManifest
        {
            FileVersion = YooAssetSettings.ManifestFileVersion,
            EnableAddressable = false,
            LocationToLower = false,
            IncludeAssetGUID = false,
            OutputNameStyle = 1,
            BuildPipeline = context.Pipeline,
            PackageName = context.PackageName,
            PackageVersion = context.PackageVersion,
            AssetList = new List<PackageAsset>(context.CollectedFiles.Count),
            BundleList = new List<PackageBundle>(context.CollectedFiles.Count)
        };

        for (var index = 0; index < context.CollectedFiles.Count; index++)
        {
            var sourceFile = context.CollectedFiles[index];
            var destinationFile = context.DestinationFiles[index];
            var bundleName = NormalizePathForDisplay(Path.GetRelativePath(context.PackageRoot, destinationFile));
            var fileHash = HashUtility.FileMD5(destinationFile);
            var fileCrc = HashUtility.FileCRC32(destinationFile);
            var fileSize = new FileInfo(destinationFile).Length;
            runtimeManifest.BundleList.Add(new PackageBundle
            {
                BundleName = bundleName,
                UnityCRC = 0,
                FileHash = fileHash,
                FileCRC = fileCrc,
                FileSize = fileSize,
                Encrypted = false,
                Tags = Array.Empty<string>(),
                DependIDs = Array.Empty<int>()
            });

            var assetPath = BuildRuntimeManifestAssetPath(sourceFile);
            runtimeManifest.AssetList.Add(new PackageAsset
            {
                Address = assetPath,
                AssetPath = assetPath,
                AssetGUID = string.Empty,
                AssetTags = Array.Empty<string>(),
                BundleID = index
            });
        }

        context.RuntimeVersionFilePath = Path.Combine(context.PackageRoot, YooAssetSettingsData.GetPackageVersionFileName(context.PackageName));
        context.RuntimeManifestFilePath = Path.Combine(context.PackageRoot, YooAssetSettingsData.GetManifestBinaryFileName(context.PackageName, context.PackageVersion));
        context.RuntimeHashFilePath = Path.Combine(context.PackageRoot, YooAssetSettingsData.GetPackageHashFileName(context.PackageName, context.PackageVersion));
        SerializeRuntimeManifestBinary(context.RuntimeManifestFilePath, runtimeManifest);
        var runtimeManifestHash = HashUtility.FileMD5(context.RuntimeManifestFilePath);
        File.WriteAllText(context.RuntimeVersionFilePath, context.PackageVersion, Encoding.UTF8);
        File.WriteAllText(context.RuntimeHashFilePath, runtimeManifestHash, Encoding.UTF8);
        AppendBuilderLog($"{BuilderStageCompletedPrefix}{BuilderStageRuntimeLink}");
    }

    private void RunVerifyBuildStage(BuildOrchestrationContext context)
    {
        AppendBuilderLog($"{BuilderStageStartPrefix}{BuilderStageVerify}");
        if (!Directory.Exists(context.FilesRoot))
        {
            throw new InvalidOperationException(BuilderVerifyErrorOutputRootMissing);
        }

        if (!File.Exists(context.ManifestPath))
        {
            throw new InvalidOperationException(BuilderVerifyErrorManifestMissing);
        }

        if (!File.Exists(context.VersionFilePath))
        {
            throw new InvalidOperationException(BuilderVerifyErrorVersionFileMissing);
        }

        if (!File.Exists(context.RuntimeVersionFilePath))
        {
            throw new InvalidOperationException(BuilderVerifyErrorRuntimeVersionFileMissing);
        }

        if (!File.Exists(context.RuntimeManifestFilePath))
        {
            throw new InvalidOperationException(BuilderVerifyErrorRuntimeManifestFileMissing);
        }

        if (!File.Exists(context.RuntimeHashFilePath))
        {
            throw new InvalidOperationException(BuilderVerifyErrorRuntimeHashFileMissing);
        }

        if (context.CopiedCount != context.CollectedFiles.Count)
        {
            throw new InvalidOperationException(BuilderVerifyErrorFileCountMismatch);
        }

        foreach (var destinationFile in context.DestinationFiles)
        {
            if (!File.Exists(destinationFile))
            {
                throw new InvalidOperationException($"{BuilderVerifyErrorFileMissingPrefix}{destinationFile}");
            }
        }

        AppendBuilderLog($"{BuilderStageCompletedPrefix}{BuilderStageVerify}");
    }

    private static bool IsBuilderSourceFileIncluded(string absolutePath, string pipeline)
    {
        var normalized = NormalizePathForDisplay(absolutePath);
        if (normalized.Contains(SourceFilterIgnoreDirectory, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (normalized.EndsWith(SourceFilterIgnoreImportExtension, StringComparison.OrdinalIgnoreCase) ||
            normalized.EndsWith(SourceFilterIgnoreUidExtension, StringComparison.OrdinalIgnoreCase) ||
            normalized.EndsWith(SourceFilterIgnoreScriptExtension, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var extension = Path.GetExtension(normalized);
        if (string.IsNullOrEmpty(extension))
        {
            return false;
        }

        if (string.Equals(pipeline, ReporterDefaultPipeline, StringComparison.OrdinalIgnoreCase))
        {
            return RawFilePipelineExtensions.Contains(extension);
        }

        return ScenePipelineExtensions.Contains(extension);
    }

    private static string GetDefaultPipelineKeywords(string pipeline)
    {
        if (string.Equals(pipeline, ReporterDefaultPipeline, StringComparison.OrdinalIgnoreCase))
        {
            return PipelineKeywordRawFile;
        }

        return PipelineKeywordSceneOnly;
    }

    private static CollectorRuleModel ParseCollectorRuleModel(string keywordsText)
    {
        var model = new CollectorRuleModel();
        if (string.IsNullOrWhiteSpace(keywordsText))
        {
            return model;
        }

        var tokens = keywordsText.Split(KeywordSeparator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        foreach (var token in tokens)
        {
            if (token.StartsWith("include:", StringComparison.OrdinalIgnoreCase))
            {
                model.IncludeKeywords.Add(token.Substring("include:".Length));
                continue;
            }

            if (token.StartsWith("exclude:", StringComparison.OrdinalIgnoreCase))
            {
                model.ExcludeKeywords.Add(token.Substring("exclude:".Length));
                continue;
            }

            if (token.StartsWith("ext:", StringComparison.OrdinalIgnoreCase))
            {
                var extension = NormalizeCollectorExtension(token.Substring("ext:".Length));
                if (!string.IsNullOrEmpty(extension))
                {
                    model.IncludeExtensions.Add(extension);
                }

                continue;
            }

            if (token.StartsWith("exclude_ext:", StringComparison.OrdinalIgnoreCase))
            {
                var extension = NormalizeCollectorExtension(token.Substring("exclude_ext:".Length));
                if (!string.IsNullOrEmpty(extension))
                {
                    model.ExcludeExtensions.Add(extension);
                }

                continue;
            }

            if (token.StartsWith("!", StringComparison.OrdinalIgnoreCase))
            {
                var excluded = token.Substring(1);
                if (!string.IsNullOrEmpty(excluded))
                {
                    model.ExcludeKeywords.Add(excluded);
                }

                continue;
            }

            if (token.StartsWith(".", StringComparison.OrdinalIgnoreCase))
            {
                var extension = NormalizeCollectorExtension(token);
                if (!string.IsNullOrEmpty(extension))
                {
                    model.IncludeExtensions.Add(extension);
                }

                continue;
            }

            model.IncludeKeywords.Add(token);
        }

        return model;
    }

    private static string NormalizeCollectorExtension(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value.Trim();
        if (!normalized.StartsWith(".", StringComparison.Ordinal))
        {
            normalized = $".{normalized}";
        }

        return normalized.ToLowerInvariant();
    }

    private static bool IsCollectorRuleMatch(string filePath, CollectorRuleModel model)
    {
        var normalizedPath = NormalizePathForDisplay(filePath);
        var extension = Path.GetExtension(normalizedPath).ToLowerInvariant();

        if (model.IncludeExtensions.Count > 0 && !model.IncludeExtensions.Contains(extension))
        {
            return false;
        }

        if (model.ExcludeExtensions.Contains(extension))
        {
            return false;
        }

        if (model.IncludeKeywords.Count > 0)
        {
            var matchedInclude = false;
            foreach (var keyword in model.IncludeKeywords)
            {
                if (normalizedPath.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    matchedInclude = true;
                    break;
                }
            }

            if (!matchedInclude)
            {
                return false;
            }
        }

        foreach (var keyword in model.ExcludeKeywords)
        {
            if (normalizedPath.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    private bool TryCollectFiles(string scanRoot, string keywordsText, string pipeline, int scanLimit, out string globalScanRoot, out List<string> matchedFiles, out int scannedCount, out bool scanLimitHit, out string errorMessage)
    {
        matchedFiles = new List<string>();
        scannedCount = 0;
        scanLimitHit = false;
        errorMessage = string.Empty;
        globalScanRoot = string.Empty;

        if (string.IsNullOrEmpty(scanRoot))
        {
            errorMessage = ScanRootEmptyError;
            return false;
        }

        globalScanRoot = _editorPlatformBridge.GlobalizePath(scanRoot);
        if (!Directory.Exists(globalScanRoot))
        {
            errorMessage = $"{ScanRootNotExistsErrorPrefix}{globalScanRoot}";
            return false;
        }

        var collectorRuleModel = ParseCollectorRuleModel(keywordsText);
        foreach (var file in Directory.EnumerateFiles(globalScanRoot, FileSearchPatternAll, SearchOption.AllDirectories))
        {
            scannedCount++;
            if (scannedCount > scanLimit)
            {
                scanLimitHit = true;
                break;
            }

            if (!IsBuilderSourceFileIncluded(file, pipeline))
            {
                continue;
            }

            if (!IsCollectorRuleMatch(file, collectorRuleModel))
            {
                continue;
            }

            matchedFiles.Add(file);
        }

        return true;
    }

    private string BuildManifestText(string packageName, string pipeline, string packageVersion, string sourceRoot, string filesRoot, List<string> collectedFiles, DateTime buildTime)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"{ManifestFieldPackageName}={packageName}");
        builder.AppendLine($"{ManifestFieldPipeline}={pipeline}");
        builder.AppendLine($"{ManifestFieldBuildVersion}={packageVersion}");
        builder.AppendLine($"{ManifestFieldBuildTime}={buildTime.ToString(ExportIsoDateTimeFormat)}");
        builder.AppendLine($"{ManifestFieldSourceRoot}={NormalizePathForDisplay(sourceRoot)}");
        builder.AppendLine($"{ManifestFieldOutputFilesRoot}={NormalizePathForDisplay(filesRoot)}");
        builder.AppendLine($"{ManifestFieldFileCount}={collectedFiles.Count}");
        builder.AppendLine(ManifestSectionFiles);

        foreach (var file in collectedFiles)
        {
            builder.AppendLine(Path.GetFileName(file));
            builder.AppendLine(ToProjectDisplayPath(file));
        }

        return builder.ToString();
    }

    private string BuildVersionText(BuildOrchestrationContext context)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"{ManifestFieldPackageName}={context.PackageName}");
        builder.AppendLine($"{ManifestFieldPipeline}={context.Pipeline}");
        builder.AppendLine($"{ManifestFieldBuildVersion}={context.PackageVersion}");
        builder.AppendLine($"{ManifestFieldBuildTime}={context.BuildTime.ToString(ExportIsoDateTimeFormat)}");
        builder.AppendLine($"{ManifestFieldOutputFilesRoot}={NormalizePathForDisplay(context.FilesRoot)}");
        builder.AppendLine($"{ManifestFieldFileCount}={context.CopiedCount}");
        builder.AppendLine($"{BuilderManifestFileName}={Path.GetFileName(context.ManifestPath)}");
        return builder.ToString();
    }

    private string BuildRuntimeManifestAssetPath(string sourceFile)
    {
        var sourceAssetPath = ToProjectDisplayPath(sourceFile);
        if (string.IsNullOrEmpty(sourceAssetPath))
        {
            return NormalizePathForDisplay(sourceFile);
        }

        return sourceAssetPath;
    }

    private void SerializeRuntimeManifestBinary(string savePath, PackageManifest manifest)
    {
        using var fileStream = new FileStream(savePath, System.IO.FileMode.Create, System.IO.FileAccess.Write);
        using var writer = new BinaryWriter(fileStream, Encoding.UTF8, false);
        writer.Write(YooAssetSettings.ManifestFileSign);
        WriteUtf8(writer, manifest.FileVersion);
        writer.Write(manifest.EnableAddressable);
        writer.Write(manifest.LocationToLower);
        writer.Write(manifest.IncludeAssetGUID);
        writer.Write(manifest.OutputNameStyle);
        WriteUtf8(writer, manifest.BuildPipeline);
        WriteUtf8(writer, manifest.PackageName);
        WriteUtf8(writer, manifest.PackageVersion);
        writer.Write(manifest.AssetList.Count);
        foreach (var packageAsset in manifest.AssetList)
        {
            WriteUtf8(writer, packageAsset.Address);
            WriteUtf8(writer, packageAsset.AssetPath);
            WriteUtf8(writer, packageAsset.AssetGUID);
            WriteUtf8Array(writer, packageAsset.AssetTags);
            writer.Write(packageAsset.BundleID);
        }

        writer.Write(manifest.BundleList.Count);
        foreach (var packageBundle in manifest.BundleList)
        {
            WriteUtf8(writer, packageBundle.BundleName);
            writer.Write(packageBundle.UnityCRC);
            WriteUtf8(writer, packageBundle.FileHash);
            WriteUtf8(writer, packageBundle.FileCRC);
            writer.Write(packageBundle.FileSize);
            writer.Write(packageBundle.Encrypted);
            WriteUtf8Array(writer, packageBundle.Tags);
            WriteInt32Array(writer, packageBundle.DependIDs);
        }

        writer.Flush();
    }

    private void WriteUtf8(BinaryWriter writer, string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            writer.Write((ushort)0);
            return;
        }

        var utf8Bytes = Encoding.UTF8.GetBytes(value);
        if (utf8Bytes.Length > ushort.MaxValue)
        {
            throw new InvalidOperationException("Runtime 清单字段长度超过 UInt16 上限。");
        }

        writer.Write((ushort)utf8Bytes.Length);
        writer.Write(utf8Bytes);
    }

    private void WriteUtf8Array(BinaryWriter writer, string[] values)
    {
        if (values == null || values.Length == 0)
        {
            writer.Write((ushort)0);
            return;
        }

        if (values.Length > ushort.MaxValue)
        {
            throw new InvalidOperationException("Runtime 清单数组长度超过 UInt16 上限。");
        }

        writer.Write((ushort)values.Length);
        foreach (var value in values)
        {
            WriteUtf8(writer, value);
        }
    }

    private void WriteInt32Array(BinaryWriter writer, int[] values)
    {
        if (values == null || values.Length == 0)
        {
            writer.Write((ushort)0);
            return;
        }

        if (values.Length > ushort.MaxValue)
        {
            throw new InvalidOperationException("Runtime 清单数组长度超过 UInt16 上限。");
        }

        writer.Write((ushort)values.Length);
        foreach (var value in values)
        {
            writer.Write(value);
        }
    }

    private void RunCollectorPrototype()
    {
        var scanRoot = _collectorScanRootInput?.Text?.Trim() ?? string.Empty;
        var keywordsText = _collectorKeywordInput?.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(scanRoot))
        {
            SetStatus(CollectorParamErrorScanRootStatus);
            AppendCollectorLog(CollectorParamErrorScanRootLog);
            return;
        }

        var pipeline = _builderPipelineOptions == null ? ReporterDefaultPipeline : _builderPipelineOptions.GetItemText(_builderPipelineOptions.Selected);
        if (string.IsNullOrEmpty(keywordsText))
        {
            keywordsText = GetDefaultPipelineKeywords(pipeline);
            if (_collectorKeywordInput != null)
            {
                _collectorKeywordInput.Text = keywordsText;
            }
        }

        if (!TryCollectFiles(scanRoot, keywordsText, pipeline, CollectorScanLimit, out var globalScanRoot, out var matchedFiles, out var scannedCount, out var scanLimitHit, out var errorMessage))
        {
            SetStatus(CollectorRunStatusScanRootUnavailable);
            AppendCollectorLog($"{BuilderRunLogFailedPrefix}{errorMessage}");
            return;
        }

        if (_collectorPreviewView != null)
        {
            _collectorPreviewView.Clear();
        }

        for (var i = 0; i < matchedFiles.Count && i < CollectorPreviewLimit; i++)
        {
            if (_collectorPreviewView != null)
            {
                _collectorPreviewView.AppendText($"{ToProjectDisplayPath(matchedFiles[i])}{PluginLogLineTerminator}");
            }
        }

        AppendCollectorLog(CollectorRunLogStart);
        AppendCollectorLog($"{CollectorRunLogScanRootPrefix}{scanRoot}");
        AppendCollectorLog($"{CollectorRunLogGlobalRootPrefix}{globalScanRoot}");
        AppendCollectorLog($"{CollectorRunLogPipelinePrefix}{pipeline}");
        AppendCollectorLog($"{CollectorRunLogKeywordsPrefix}{keywordsText}");
        AppendCollectorLog($"{CollectorRunLogScannedCountPrefix}{Math.Min(scannedCount, CollectorScanLimit)}");
        AppendCollectorLog($"{CollectorRunLogMatchedCountPrefix}{matchedFiles.Count}");
        AppendCollectorLog($"{CollectorRunLogPreviewLimitPrefix}{CollectorPreviewLimit}");
        if (scanLimitHit)
        {
            AppendCollectorLog($"{CollectorRunLogScanLimitHitPrefix}{CollectorScanLimit}");
        }

        SetStatus($"{CollectorRunStatusCompletedPrefix}{matchedFiles.Count}{CollectorRunStatusCompletedSuffix}");
    }

    private string ToProjectDisplayPath(string absolutePath)
    {
        var projectRoot = _editorPlatformBridge.GlobalizePath(ProjectDisplayPrefix).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        if (absolutePath.StartsWith(projectRoot, StringComparison.OrdinalIgnoreCase))
        {
            var relative = absolutePath.Substring(projectRoot.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            return $"{ProjectDisplayPrefix}{NormalizePathForDisplay(relative)}";
        }

        return NormalizePathForDisplay(absolutePath);
    }

    private static string NormalizePathForDisplay(string path)
    {
        return path.Replace('\\', '/');
    }

    private void AppendCollectorLog(string message)
    {
        AppendLogEntry(_collectorLogView, message);
    }

    private void RunReporterPrototype()
    {
        var scanRoot = _reporterScanRootInput?.Text?.Trim() ?? string.Empty;
        var keyword = _reporterKeywordInput?.Text?.Trim() ?? string.Empty;
        var pipeline = _builderPipelineOptions == null ? ReporterDefaultPipeline : _builderPipelineOptions.GetItemText(_builderPipelineOptions.Selected);
        var sortMode = _reporterSortOptions == null ? ReporterDefaultSortMode : _reporterSortOptions.GetItemText(_reporterSortOptions.Selected);
        if (string.IsNullOrEmpty(scanRoot))
        {
            scanRoot = _collectorScanRootInput?.Text?.Trim() ?? ReporterDefaultScanRoot;
        }

        var keywordsText = keyword;
        if (string.IsNullOrEmpty(keywordsText))
        {
            keywordsText = _collectorKeywordInput?.Text?.Trim() ?? string.Empty;
        }

        if (string.IsNullOrEmpty(keywordsText))
        {
            keywordsText = GetDefaultPipelineKeywords(pipeline);
        }

        var scanLimit = ReporterScanLimit;
        if (!TryCollectFiles(scanRoot, keywordsText, pipeline, scanLimit, out var globalScanRoot, out var matchedFiles, out var scannedCount, out var scanLimitHit, out var errorMessage))
        {
            SetStatus(ReporterRunStatusScanRootUnavailable);
            AppendReporterLog($"{ReporterRunLogFailedPrefix}{errorMessage}");
            return;
        }

        var extensionMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var file in matchedFiles)
        {
            var extension = Path.GetExtension(file);
            if (string.IsNullOrEmpty(extension))
            {
                extension = ReporterNoExtensionPlaceholder;
            }

            if (!extensionMap.TryAdd(extension, 1))
            {
                extensionMap[extension]++;
            }
        }

        var lines = BuildExtensionSummary(extensionMap, sortMode, ReporterSummaryTopExtensionCount);
        _reporterLastSummary = BuildReporterSummaryText(scanRoot, globalScanRoot, pipeline, keyword, keywordsText, sortMode, scannedCount, scanLimit, matchedFiles.Count, lines, scanLimitHit);
        _reporterSummaryFields = ParseSummaryFields(_reporterLastSummary);

        if (_reporterSummaryView != null)
        {
            _reporterSummaryView.Clear();
            _reporterSummaryView.AppendText(_reporterLastSummary);
        }

        AppendReporterLog(ReporterRunLogStart);
        AppendReporterLog($"{ReporterRunLogScanRootPrefix}{scanRoot}");
        AppendReporterLog($"{ReporterRunLogPipelinePrefix}{pipeline}");
        AppendReporterLog($"{ReporterRunLogFilterKeywordPrefix}{keyword}");
        AppendReporterLog($"{ReporterRunLogCollectorKeywordsPrefix}{keywordsText}");
        AppendReporterLog($"{ReporterRunLogSortModePrefix}{sortMode}");
        AppendReporterLog($"{ReporterRunLogScannedCountPrefix}{Math.Min(scannedCount, scanLimit)}");
        AppendReporterLog($"{ReporterRunLogMatchedCountPrefix}{matchedFiles.Count}");
        SetStatus($"{ReporterRunStatusCompletedPrefix}{matchedFiles.Count}{ReporterRunStatusCompletedSuffix}");
    }

    private void LoadLatestBuildReport()
    {
        AppendReporterLog(ReporterLoadBuildReportLogStart);
        if (!TryResolveLatestBuildPackageRoot(out var packageRoot))
        {
            SetStatus(ReporterLoadBuildReportUnavailable);
            AppendReporterLog(ReporterLoadBuildReportUnavailable);
            return;
        }

        var manifestPath = Path.Combine(packageRoot, BuilderManifestFileName);
        var versionPath = Path.Combine(packageRoot, BuilderVersionFileName);
        var manifestAvailable = File.Exists(manifestPath);
        var versionAvailable = File.Exists(versionPath);
        if (!manifestAvailable)
        {
            AppendReporterLog(ReporterLoadBuildReportMissingManifest);
        }

        if (!versionAvailable)
        {
            AppendReporterLog(ReporterLoadBuildReportMissingVersion);
        }

        var manifestFields = ReadKeyValueFile(manifestPath);
        var versionFields = ReadKeyValueFile(versionPath);
        var packageName = GetBuildField(versionFields, manifestFields, ManifestFieldPackageName);
        var packageVersion = GetBuildField(versionFields, manifestFields, ManifestFieldBuildVersion);
        var pipeline = GetBuildField(versionFields, manifestFields, ManifestFieldPipeline);
        var runtimeVersionFilePath = string.IsNullOrEmpty(packageName) ? string.Empty : Path.Combine(packageRoot, YooAssetSettingsData.GetPackageVersionFileName(packageName));
        var runtimeManifestFilePath = string.IsNullOrEmpty(packageName) || string.IsNullOrEmpty(packageVersion)
            ? string.Empty
            : Path.Combine(packageRoot, YooAssetSettingsData.GetManifestBinaryFileName(packageName, packageVersion));
        var runtimeHashFilePath = string.IsNullOrEmpty(packageName) || string.IsNullOrEmpty(packageVersion)
            ? string.Empty
            : Path.Combine(packageRoot, YooAssetSettingsData.GetPackageHashFileName(packageName, packageVersion));
        var runtimeVersionAvailable = !string.IsNullOrEmpty(runtimeVersionFilePath) && File.Exists(runtimeVersionFilePath);
        var runtimeManifestAvailable = !string.IsNullOrEmpty(runtimeManifestFilePath) && File.Exists(runtimeManifestFilePath);
        var runtimeHashAvailable = !string.IsNullOrEmpty(runtimeHashFilePath) && File.Exists(runtimeHashFilePath);
        if (!runtimeVersionAvailable)
        {
            AppendReporterLog(ReporterLoadBuildReportRuntimeVersionMissing);
        }

        if (!runtimeHashAvailable)
        {
            AppendReporterLog(ReporterLoadBuildReportRuntimeHashMissing);
        }

        if (!runtimeManifestAvailable)
        {
            AppendReporterLog(ReporterLoadBuildReportRuntimeManifestMissing);
        }

        var expectedHash = runtimeHashAvailable ? File.ReadAllText(runtimeHashFilePath).Trim() : string.Empty;
        var actualHash = runtimeManifestAvailable ? HashUtility.FileMD5(runtimeManifestFilePath) : string.Empty;
        var runtimeHashMatch = runtimeHashAvailable && runtimeManifestAvailable && string.Equals(expectedHash, actualHash, StringComparison.OrdinalIgnoreCase);
        if (runtimeHashAvailable && runtimeManifestAvailable && !runtimeHashMatch)
        {
            AppendReporterLog(ReporterLoadBuildReportRuntimeHashMismatch);
        }

        var runtimeVersionValue = runtimeVersionAvailable ? File.ReadAllText(runtimeVersionFilePath).Trim() : string.Empty;
        var runtimeVersionMatch = runtimeVersionAvailable && !string.IsNullOrEmpty(packageVersion) && string.Equals(runtimeVersionValue, packageVersion, StringComparison.Ordinal);
        if (runtimeVersionAvailable && !string.IsNullOrEmpty(packageVersion) && !runtimeVersionMatch)
        {
            AppendReporterLog(ReporterLoadBuildReportRuntimeVersionMismatch);
        }

        _lastBuildOutputDirectory = packageRoot;
        _lastBuildManifestPath = manifestPath;
        _lastBuildPackageName = packageName;
        _lastBuildPipeline = pipeline;
        _lastBuildFileCount = ParseIntField(GetBuildField(versionFields, manifestFields, ManifestFieldFileCount));
        _lastBuildScannedCount = _lastBuildFileCount;
        _lastBuildScanLimitHit = false;
        _reporterLastSummary = BuildBuildReportSummaryText(packageRoot, packageName, packageVersion, pipeline, manifestAvailable, versionAvailable, runtimeVersionAvailable, runtimeHashAvailable, runtimeManifestAvailable, runtimeHashMatch, runtimeVersionMatch, expectedHash, actualHash);
        _reporterSummaryFields = ParseSummaryFields(_reporterLastSummary);
        if (_reporterSummaryView != null)
        {
            _reporterSummaryView.Clear();
            _reporterSummaryView.AppendText(_reporterLastSummary);
        }

        SetStatus(ReporterLoadBuildReportCompleted);
        AppendReporterLog($"{ReporterOpenManifestLogPrefix}{NormalizePathForDisplay(manifestPath)}");
        AppendReporterLog(ReporterLoadBuildReportCompleted);
    }

    private void ExportReporterPlaceholder()
    {
        if (string.IsNullOrEmpty(_reporterLastSummary))
        {
            SetStatus(ReporterExportStatusNoSummary);
            AppendReporterLog(ReporterExportLogNoSummary);
            return;
        }

        try
        {
            var folder = _editorPlatformBridge.GlobalizePath(ReporterExportOutputDirectory);
            Directory.CreateDirectory(folder);
            var exportTime = DateTime.Now;
            var fileName = BuildReporterExportFileName();
            var fullPath = Path.Combine(folder, fileName);
            File.WriteAllText(fullPath, BuildReporterExportText(fileName, exportTime), Encoding.UTF8);
            ResolveManifestState(out var manifestAvailable, out var manifestUnavailableReason);
            AppendReporterLog($"{ReporterExportLogSuccessPrefix}{fullPath}");
            AppendReporterLog(string.Format(ReporterExportStateLogFormat, ReporterExportStateLogPrefix, ExportFieldBuildManifestAvailable, manifestAvailable, ExportFieldBuildManifestUnavailableReason, manifestUnavailableReason));
            SetStatus(ReporterExportStatusSuccess);
        }
        catch (Exception ex)
        {
            AppendReporterLog($"{ReporterExportLogFailedPrefix}{ex.Message}");
            SetStatus(ReporterExportStatusFailed);
        }
    }

    private string BuildReporterExportFileName()
    {
        var timestamp = DateTime.Now.ToString(ExportFileNameTimestampFormat);
        if (_lastBuildTime != DateTime.MinValue)
        {
            timestamp = _lastBuildTime.ToString(ExportFileNameTimestampFormat);
        }

        var packageName = SanitizeFileNameSegment(_lastBuildPackageName);
        if (string.IsNullOrEmpty(packageName))
        {
            packageName = ExportFileNameUnknownPackage;
        }

        return $"{ExportFileNamePrefix}{ExportFileNameSeparator}{packageName}{ExportFileNameSeparator}{timestamp}{ExportFileNameExtension}";
    }

    private static string SanitizeFileNameSegment(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value.Trim();
        foreach (var invalidChar in Path.GetInvalidFileNameChars())
        {
            normalized = normalized.Replace(invalidChar, FileNameReplacementChar);
        }

        normalized = normalized.Replace(FileNameSpaceChar, FileNameReplacementChar);
        return normalized;
    }

    private void ResolveManifestState(out bool manifestAvailable, out string manifestUnavailableReason)
    {
        if (string.IsNullOrEmpty(_lastBuildOutputDirectory))
        {
            manifestAvailable = false;
            manifestUnavailableReason = ManifestUnavailableNoBuildRecord;
            return;
        }

        manifestAvailable = !string.IsNullOrEmpty(_lastBuildManifestPath) && File.Exists(_lastBuildManifestPath);
        manifestUnavailableReason = manifestAvailable ? ManifestUnavailableNone : ManifestUnavailableNotFound;
    }

    private string BuildReporterExportText(string exportFileName, DateTime exportTime)
    {
        ResolveManifestState(out var manifestAvailable, out var manifestUnavailableReason);
        var builder = new StringBuilder();
        builder.AppendLine(ExportSectionInfo);
        builder.AppendLine($"{ExportFieldExportFileName}={exportFileName}");
        builder.AppendLine($"{ExportFieldExportTime}={exportTime.ToString(ExportIsoDateTimeFormat)}");
        builder.AppendLine($"{ExportFieldBuildManifestAvailable}={manifestAvailable}");
        builder.AppendLine($"{ExportFieldBuildManifestUnavailableReason}={manifestUnavailableReason}");
        builder.AppendLine();
        builder.AppendLine(ExportSectionReporterSummary);
        builder.AppendLine(_reporterLastSummary);
        builder.AppendLine();
        builder.AppendLine(ExportSectionBuilderSnapshot);
        if (string.IsNullOrEmpty(_lastBuildOutputDirectory))
        {
            builder.AppendLine($"{SnapshotFieldState}={ManifestUnavailableNoBuildRecord}");
            builder.AppendLine($"{SnapshotFieldManifestAvailable}=false");
            builder.AppendLine($"{SnapshotFieldManifestUnavailableReason}={ManifestUnavailableNoBuildRecord}");
            builder.AppendLine();
            builder.AppendLine(ExportSectionBuildManifest);
            builder.AppendLine($"{SnapshotFieldState}={ManifestUnavailableNoBuildRecord}");
            return builder.ToString();
        }

        builder.AppendLine($"{SnapshotFieldPackageName}={_lastBuildPackageName}");
        builder.AppendLine($"{SnapshotFieldPipeline}={_lastBuildPipeline}");
        builder.AppendLine($"{SnapshotFieldBuildTime}={_lastBuildTime.ToString(ExportIsoDateTimeFormat)}");
        builder.AppendLine($"{SnapshotFieldOutputDirectory}={NormalizePathForDisplay(_lastBuildOutputDirectory)}");
        builder.AppendLine($"{SnapshotFieldManifestPath}={NormalizePathForDisplay(_lastBuildManifestPath)}");
        builder.AppendLine($"{SnapshotFieldScannedCount}={_lastBuildScannedCount}");
        builder.AppendLine($"{SnapshotFieldFileCount}={_lastBuildFileCount}");
        builder.AppendLine($"{SnapshotFieldScanLimitHit}={_lastBuildScanLimitHit}");
        builder.AppendLine($"{SnapshotFieldManifestAvailable}={manifestAvailable}");
        builder.AppendLine($"{SnapshotFieldManifestUnavailableReason}={manifestUnavailableReason}");
        builder.AppendLine();
        builder.AppendLine(ExportSectionBuildManifest);
        if (manifestAvailable)
        {
            builder.Append(File.ReadAllText(_lastBuildManifestPath));
        }
        else
        {
            builder.AppendLine($"{SnapshotFieldState}={ManifestUnavailableNotFound}");
        }

        return builder.ToString();
    }

    private void OpenLastBuildOutputDirectory()
    {
        if (string.IsNullOrEmpty(_lastBuildOutputDirectory) || !Directory.Exists(_lastBuildOutputDirectory))
        {
            SetStatus(ReporterOpenOutputUnavailable);
            AppendReporterLog(ReporterOpenOutputUnavailable);
            return;
        }

        var openPath = NormalizePathForDisplay(_lastBuildOutputDirectory);
        _editorPlatformBridge.OpenExternalPath(openPath);
        AppendReporterLog($"{ReporterOpenOutputLogPrefix}{openPath}");
        SetStatus(ReporterOpenOutputSuccess);
    }

    private void OpenLastBuildManifestFile()
    {
        if (string.IsNullOrEmpty(_lastBuildManifestPath) || !File.Exists(_lastBuildManifestPath))
        {
            SetStatus(ReporterOpenManifestUnavailable);
            AppendReporterLog(ReporterOpenManifestUnavailable);
            return;
        }

        var openPath = NormalizePathForDisplay(_lastBuildManifestPath);
        _editorPlatformBridge.OpenExternalPath(openPath);
        AppendReporterLog($"{ReporterOpenManifestLogPrefix}{openPath}");
        SetStatus(ReporterOpenManifestSuccess);
    }

    private static string BuildExtensionSummary(Dictionary<string, int> extensionMap, string sortMode, int topLimit)
    {
        var list = new List<KeyValuePair<string, int>>(extensionMap);
        if (sortMode == ExtensionSummarySortModeAsc)
        {
            list.Sort((a, b) => string.Compare(a.Key, b.Key, StringComparison.OrdinalIgnoreCase));
        }
        else
        {
            list.Sort((a, b) => b.Value.CompareTo(a.Value));
        }

        if (list.Count == 0)
        {
            return ExtensionSummaryEmpty;
        }

        var result = string.Empty;
        for (var i = 0; i < list.Count && i < topLimit; i++)
        {
            result += $"{string.Format(ExtensionSummaryLineFormat, i + 1, list[i].Key, list[i].Value)}{PluginLogLineTerminator}";
        }

        return result.TrimEnd();
    }

    private static string BuildReporterSummaryText(string scanRoot, string globalScanRoot, string pipeline, string keyword, string keywordsText, string sortMode, int scannedCount, int scanLimit, int matchedCount, string extensionLines, bool scanLimitHit)
    {
        var builder = new StringBuilder();
        builder.Append($"{ReporterSummaryScanRootPrefix}{scanRoot}");
        builder.Append($"{ReporterSummaryLineSeparator}{ReporterSummaryGlobalRootPrefix}{globalScanRoot}");
        builder.Append($"{ReporterSummaryLineSeparator}{ReporterSummaryPipelinePrefix}{pipeline}");
        builder.Append($"{ReporterSummaryLineSeparator}{ReporterSummaryFilterKeywordPrefix}{keyword}");
        builder.Append($"{ReporterSummaryLineSeparator}{ReporterSummaryCollectorKeywordsPrefix}{keywordsText}");
        builder.Append($"{ReporterSummaryLineSeparator}{ReporterSummarySortModePrefix}{sortMode}");
        builder.Append($"{ReporterSummaryLineSeparator}{ReporterSummaryScannedFilesPrefix}{Math.Min(scannedCount, scanLimit)}");
        builder.Append($"{ReporterSummaryLineSeparator}{ReporterSummaryMatchedFilesPrefix}{matchedCount}");
        builder.Append($"{ReporterSummaryLineSeparator}{ReporterSummaryTopExtensionsPrefix}");
        builder.Append($"{ReporterSummaryLineSeparator}{extensionLines}");
        if (scanLimitHit)
        {
            builder.Append($"{ReporterSummaryLineSeparator}{ReporterSummaryScanLimitHitPrefix}{scanLimit}");
        }

        return builder.ToString();
    }

    private bool TryResolveLatestBuildPackageRoot(out string packageRoot)
    {
        if (!string.IsNullOrEmpty(_lastBuildManifestPath) && File.Exists(_lastBuildManifestPath))
        {
            packageRoot = Path.GetDirectoryName(_lastBuildManifestPath) ?? string.Empty;
            if (!string.IsNullOrEmpty(packageRoot))
            {
                return true;
            }
        }

        if (!string.IsNullOrEmpty(_lastBuildOutputDirectory) && Directory.Exists(_lastBuildOutputDirectory))
        {
            if (File.Exists(Path.Combine(_lastBuildOutputDirectory, BuilderManifestFileName)))
            {
                packageRoot = _lastBuildOutputDirectory;
                return true;
            }
        }

        var outputPathSetting = _builderOutputPathInput?.Text?.Trim();
        if (string.IsNullOrEmpty(outputPathSetting))
        {
            outputPathSetting = BuilderDefaultOutputPath;
        }

        var globalOutputRoot = _editorPlatformBridge.GlobalizePath(outputPathSetting);
        if (!Directory.Exists(globalOutputRoot))
        {
            packageRoot = string.Empty;
            return false;
        }

        var manifestFiles = Directory.GetFiles(globalOutputRoot, BuilderManifestFileName, SearchOption.AllDirectories);
        if (manifestFiles.Length == 0)
        {
            packageRoot = string.Empty;
            return false;
        }

        Array.Sort(manifestFiles, (left, right) => File.GetLastWriteTimeUtc(right).CompareTo(File.GetLastWriteTimeUtc(left)));
        packageRoot = Path.GetDirectoryName(manifestFiles[0]) ?? string.Empty;
        return !string.IsNullOrEmpty(packageRoot);
    }

    private static Dictionary<string, string> ReadKeyValueFile(string filePath)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            return result;
        }

        foreach (var rawLine in File.ReadAllLines(filePath))
        {
            var line = rawLine.Trim();
            if (string.IsNullOrEmpty(line))
            {
                continue;
            }

            var splitIndex = line.IndexOf('=');
            if (splitIndex <= 0 || splitIndex >= line.Length - 1)
            {
                continue;
            }

            var key = line.Substring(0, splitIndex).Trim();
            var value = line.Substring(splitIndex + 1).Trim();
            if (!string.IsNullOrEmpty(key))
            {
                result[key] = value;
            }
        }

        return result;
    }

    private static string GetBuildField(Dictionary<string, string> primary, Dictionary<string, string> fallback, string fieldName)
    {
        if (primary != null && primary.TryGetValue(fieldName, out var primaryValue))
        {
            return primaryValue;
        }

        if (fallback != null && fallback.TryGetValue(fieldName, out var fallbackValue))
        {
            return fallbackValue;
        }

        return string.Empty;
    }

    private static int ParseIntField(string value)
    {
        return int.TryParse(value, out var parsed) ? parsed : 0;
    }

    private static string BuildBuildReportSummaryText(string packageRoot, string packageName, string packageVersion, string pipeline, bool manifestAvailable, bool versionAvailable, bool runtimeVersionAvailable, bool runtimeHashAvailable, bool runtimeManifestAvailable, bool runtimeHashMatch, bool runtimeVersionMatch, string expectedHash, string actualHash)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"{ManifestFieldPackageName}={packageName}");
        builder.AppendLine($"{ManifestFieldBuildVersion}={packageVersion}");
        builder.AppendLine($"{ManifestFieldPipeline}={pipeline}");
        builder.AppendLine($"{ReportFieldPackageRoot}={NormalizePathForDisplay(packageRoot)}");
        builder.AppendLine($"{ReportFieldManifestAvailable}={manifestAvailable}");
        builder.AppendLine($"{ReportFieldVersionAvailable}={versionAvailable}");
        builder.AppendLine($"{ReportFieldRuntimeVersionAvailable}={runtimeVersionAvailable}");
        builder.AppendLine($"{ReportFieldRuntimeHashAvailable}={runtimeHashAvailable}");
        builder.AppendLine($"{ReportFieldRuntimeManifestAvailable}={runtimeManifestAvailable}");
        builder.AppendLine($"{ReportFieldRuntimeHashMatch}={runtimeHashMatch}");
        builder.AppendLine($"{ReportFieldRuntimeVersionMatch}={runtimeVersionMatch}");
        builder.AppendLine($"{ReportFieldRuntimeHashValue}={expectedHash}");
        builder.Append($"{ReportFieldRuntimeActualHashValue}={actualHash}");
        return builder.ToString();
    }

    private void AppendReporterLog(string message)
    {
        AppendLogEntry(_reporterLogView, message);
    }

    private void RefreshDebuggerSnapshot()
    {
        var editorHint = _editorPlatformBridge.IsEditorHint();
        var timeScale = _editorPlatformBridge.GetTimeScale();
        var fps = _editorPlatformBridge.GetFramesPerSecond();
        var osName = _editorPlatformBridge.GetOsName();
        var timeScaleText = timeScale.ToString(DebuggerTimeScaleNumberFormat);
        var userPath = _editorPlatformBridge.GlobalizePath(DebuggerPathCheckKeyUser);
        var resPath = _editorPlatformBridge.GlobalizePath(DebuggerPathCheckKeyRes);
        var userExists = Directory.Exists(userPath);
        var resExists = Directory.Exists(resPath);

        if (_debuggerStatusLabel != null)
        {
            _debuggerStatusLabel.Text = string.Format(DebuggerStatusSnapshotFormat, editorHint, fps, timeScaleText);
        }

        if (_debuggerPathLabel != null)
        {
            _debuggerPathLabel.Text = string.Format(DebuggerPathSnapshotFormat, resPath, userPath);
        }

        AppendDebuggerLog(DebuggerLogSnapshotRefreshed);
        AppendDebuggerLog($"{DebuggerLogOsPrefix}{osName}");
        AppendDebuggerLog($"{DebuggerLogEditorHintPrefix}{editorHint}");
        AppendDebuggerLog($"{DebuggerLogFpsPrefix}{fps}");
        AppendDebuggerLog($"{DebuggerLogTimeScalePrefix}{timeScaleText}");
        AppendDebuggerLog($"{DebuggerLogResExistsPrefix}{resExists}");
        AppendDebuggerLog($"{DebuggerLogUserExistsPrefix}{userExists}");
        SetStatus(DebuggerStatusSnapshotRefreshed);
    }

    private void RunDebuggerPathCheck()
    {
        var checks = new Dictionary<string, string>
        {
            [DebuggerPathCheckKeyRes] = _editorPlatformBridge.GlobalizePath(DebuggerPathCheckKeyRes),
            [DebuggerPathCheckKeyUser] = _editorPlatformBridge.GlobalizePath(DebuggerPathCheckKeyUser),
            [DebuggerPathCheckKeyBuilds] = _editorPlatformBridge.GlobalizePath(DebuggerPathCheckKeyBuilds),
            [DebuggerPathCheckKeyReports] = _editorPlatformBridge.GlobalizePath(DebuggerPathCheckKeyReports)
        };

        var passCount = 0;
        foreach (var pair in checks)
        {
            var exists = Directory.Exists(pair.Value);
            if (exists)
            {
                passCount++;
            }

            AppendDebuggerLog(string.Format(DebuggerPathCheckLogFormat, pair.Key, pair.Value, exists));
        }

        SetStatus(string.Format(DebuggerStatusPathCheckFormat, passCount, checks.Count));
    }

    private void RunRuntimeDebugCommand()
    {
        var command = _debuggerCommandInput?.Text?.Trim();
        if (string.IsNullOrEmpty(command))
        {
            command = DebuggerRuntimeCommandDefault;
        }

        var commandParam = _debuggerCommandParamInput?.Text?.Trim() ?? string.Empty;
        AppendDebuggerLog($"{DebuggerLogRuntimeCommandPrefix}{command}");
        AppendDebuggerLog($"{DebuggerLogRuntimeParamPrefix}{commandParam}");

        if (!RemoteCommand.TryParseCommandType(command, out var commandType))
        {
            var invalidMessage = $"Unsupported command \"{command}\"";
            AppendDebuggerLog($"{DebuggerLogRuntimeResultPrefix}{invalidMessage}");
            SetStatus($"{DebuggerStatusRuntimeCommandInvalidPrefix}{command}");
            return;
        }

        var remoteCommand = new RemoteCommand
        {
            CommandType = commandType,
            CommandParam = commandParam
        };
        var commandData = RemoteCommand.Serialize(remoteCommand);
        AppendDebuggerLog($"{DebuggerLogRuntimeProtocolSendBytesPrefix}{commandData.Length}");

        var success = YooAssets.TryExecuteDebugCommand(commandData, out var reportData, out var message);
        if (!success)
        {
            AppendDebuggerLog($"{DebuggerLogRuntimeResultPrefix}{message}");
            SetStatus($"{DebuggerStatusRuntimeCommandFailedPrefix}{message}");
            return;
        }

        AppendDebuggerLog($"{DebuggerLogRuntimeProtocolReceiveBytesPrefix}{reportData.Length}");
        AppendDebuggerLog($"{DebuggerLogRuntimeResultPrefix}{message}");
        var report = DebugReport.Deserialize(reportData);
        _lastRuntimeDebugReport = report;
        AppendRuntimeDebugReport(report);
        SetStatus(DebuggerStatusRuntimeCommandCompleted);
    }

    private void RunDiagnosticFieldAlignmentCheck()
    {
        AppendDebuggerLog(DebuggerLogAlignmentStart);
        var missingFields = new List<string>();
        var invalidFields = new List<string>();
        ValidateReporterSummaryAlignment(missingFields, invalidFields);
        ValidateRuntimeReportAlignment(missingFields, invalidFields);

        for (var index = 0; index < missingFields.Count; index++)
        {
            AppendDebuggerLog($"{DebuggerLogAlignmentMissingPrefix}{missingFields[index]}");
        }

        for (var index = 0; index < invalidFields.Count; index++)
        {
            AppendDebuggerLog($"{DebuggerLogAlignmentInvalidPrefix}{invalidFields[index]}");
        }

        if (missingFields.Count == 0 && invalidFields.Count == 0)
        {
            AppendDebuggerLog(DebuggerLogAlignmentPassed);
            SetStatus(DebuggerStatusAlignmentPassed);
            return;
        }

        SetStatus($"{DebuggerStatusAlignmentFailedPrefix}缺失{missingFields.Count}项，无效{invalidFields.Count}项");
    }

    private void ValidateReporterSummaryAlignment(List<string> missingFields, List<string> invalidFields)
    {
        var requiredFields = new[]
        {
            ManifestFieldPackageName,
            ManifestFieldBuildVersion,
            ManifestFieldPipeline,
            ReportFieldRuntimeVersionAvailable,
            ReportFieldRuntimeHashAvailable,
            ReportFieldRuntimeManifestAvailable,
            ReportFieldRuntimeHashMatch,
            ReportFieldRuntimeVersionMatch
        };

        for (var index = 0; index < requiredFields.Length; index++)
        {
            var field = requiredFields[index];
            if (!_reporterSummaryFields.TryGetValue(field, out var value))
            {
                missingFields.Add($"Reporter.{field}");
                continue;
            }

            if ((field == ManifestFieldPackageName || field == ManifestFieldBuildVersion || field == ManifestFieldPipeline) && string.IsNullOrWhiteSpace(value))
            {
                invalidFields.Add($"Reporter.{field}");
            }
        }
    }

    private void ValidateRuntimeReportAlignment(List<string> missingFields, List<string> invalidFields)
    {
        if (_lastRuntimeDebugReport == null)
        {
            missingFields.Add("Runtime.DebugReport");
            return;
        }

        if (_lastRuntimeDebugReport.PackageDatas == null)
        {
            missingFields.Add("Runtime.PackageDatas");
            return;
        }

        for (var packageIndex = 0; packageIndex < _lastRuntimeDebugReport.PackageDatas.Count; packageIndex++)
        {
            var packageData = _lastRuntimeDebugReport.PackageDatas[packageIndex];
            if (packageData == null)
            {
                invalidFields.Add($"Runtime.PackageDatas[{packageIndex}]");
                continue;
            }

            if (string.IsNullOrWhiteSpace(packageData.PackageName))
            {
                invalidFields.Add($"Runtime.PackageDatas[{packageIndex}].PackageName");
            }

            if (packageData.ProviderInfos == null)
            {
                missingFields.Add($"Runtime.PackageDatas[{packageIndex}].ProviderInfos");
                continue;
            }

            for (var providerIndex = 0; providerIndex < packageData.ProviderInfos.Count; providerIndex++)
            {
                var provider = packageData.ProviderInfos[providerIndex];
                if (provider == null)
                {
                    invalidFields.Add($"Runtime.PackageDatas[{packageIndex}].ProviderInfos[{providerIndex}]");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(provider.AssetPath))
                {
                    invalidFields.Add($"Runtime.PackageDatas[{packageIndex}].ProviderInfos[{providerIndex}].AssetPath");
                }

                if (string.IsNullOrWhiteSpace(provider.Status))
                {
                    invalidFields.Add($"Runtime.PackageDatas[{packageIndex}].ProviderInfos[{providerIndex}].Status");
                }
            }
        }
    }

    private static Dictionary<string, string> ParseSummaryFields(string text)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrEmpty(text))
        {
            return result;
        }

        var lines = text.Split('\n');
        for (var index = 0; index < lines.Length; index++)
        {
            var line = lines[index].Trim();
            if (string.IsNullOrEmpty(line))
            {
                continue;
            }

            var separator = line.IndexOf('=');
            if (separator <= 0)
            {
                continue;
            }

            var key = line.Substring(0, separator).Trim();
            var value = line.Substring(separator + 1).Trim();
            if (string.IsNullOrEmpty(key))
            {
                continue;
            }

            result[key] = value;
        }

        return result;
    }

    private void AppendRuntimeDebugReport(DebugReport report)
    {
        if (report == null)
        {
            return;
        }

        AppendDebuggerLog($"{DebuggerLogRuntimeFramePrefix}{report.FrameCount}");
        for (var packageIndex = 0; packageIndex < report.PackageDatas.Count; packageIndex++)
        {
            var packageData = report.PackageDatas[packageIndex];
            if (packageData == null)
            {
                continue;
            }

            AppendDebuggerLog($"{DebuggerLogRuntimePackagePrefix}{packageData.PackageName}");
            var providerCount = packageData.ProviderInfos == null ? 0 : packageData.ProviderInfos.Count;
            AppendDebuggerLog($"{DebuggerLogRuntimeProviderCountPrefix}{providerCount}");
            if (packageData.ProviderInfos == null)
            {
                continue;
            }

            for (var providerIndex = 0; providerIndex < packageData.ProviderInfos.Count; providerIndex++)
            {
                var providerInfo = packageData.ProviderInfos[providerIndex];
                if (providerInfo == null)
                {
                    continue;
                }

                AppendDebuggerLog(
                    $"{DebuggerLogRuntimeProviderPrefix}{providerInfo.AssetPath}{DebuggerLogRuntimeProviderDetailSeparator}{DebuggerLogRuntimeProviderRefPrefix}{providerInfo.RefCount}{DebuggerLogRuntimeProviderDetailSeparator}{DebuggerLogRuntimeProviderStatusPrefix}{providerInfo.Status}");
            }
        }
    }

    private void AppendDebuggerLog(string message)
    {
        AppendLogEntry(_debuggerLogView, message);
    }

    private void AppendLogEntry(RichTextLabel logView, string message)
    {
        if (logView == null)
        {
            return;
        }

        var time = DateTime.Now.ToString(PluginLogTimeFormat);
        logView.AppendText($"{PluginLogEntryPrefix}{time}{PluginLogEntrySuffix}{message}{PluginLogLineTerminator}");
    }

    private void SwitchTab(int tabIndex, string moduleName)
    {
        if (_tabContainer == null)
        {
            SetStatus(TabSwitchStatusUnavailable);
            return;
        }

        if (tabIndex < 0 || tabIndex >= _tabContainer.GetTabCount())
        {
            SetStatus($"{TabSwitchStatusOutOfRangePrefix}{tabIndex}");
            return;
        }

        _tabContainer.CurrentTab = tabIndex;
        _lastOpenedTabIndex = tabIndex;
        SetStatus($"{TabSwitchStatusPrefix}{moduleName}{TabSwitchStatusSuffix}");
    }

    private void SetStatus(string message)
    {
        if (_statusView == null)
        {
            return;
        }

        _statusView.Clear();
        _statusView.AppendText(message);
    }

    /// <summary>
    /// 编辑器平台能力抽象接口
    /// </summary>
    private interface IEditorPlatformBridge
    {
        string GlobalizePath(string path);
        void OpenExternalPath(string path);
        bool IsEditorHint();
        double GetTimeScale();
        double GetFramesPerSecond();
        string GetOsName();
    }

    /// <summary>
    /// Godot 编辑器平台最小能力实现
    /// </summary>
    private sealed class GodotEditorPlatformBridge : IEditorPlatformBridge
    {
        public string GlobalizePath(string path)
        {
            return ProjectSettings.GlobalizePath(path);
        }

        public void OpenExternalPath(string path)
        {
            OS.ShellOpen(path);
        }

        public bool IsEditorHint()
        {
            return Engine.IsEditorHint();
        }

        public double GetTimeScale()
        {
            return Engine.TimeScale;
        }

        public double GetFramesPerSecond()
        {
            return Engine.GetFramesPerSecond();
        }

        public string GetOsName()
        {
            return OS.GetName();
        }
    }

    private sealed class BuildExecutionResult
    {
        public string PackageName { get; set; }
        public string Pipeline { get; set; }
        public string PackageVersion { get; set; }
        public string OutputDirectory { get; set; }
        public string ManifestPath { get; set; }
        public int FileCount { get; set; }
        public int ScannedCount { get; set; }
        public bool ScanLimitHit { get; set; }
        public DateTime BuildTime { get; set; }
        public string Summary { get; set; }
    }

    private sealed class BuildOrchestrationContext
    {
        public string PackageName { get; set; }
        public string GlobalOutputPath { get; set; }
        public string Pipeline { get; set; }
        public string ScanRoot { get; set; }
        public string KeywordsText { get; set; }
        public DateTime BuildTime { get; set; }
        public string BuildTimestamp { get; set; }
        public string PackageVersion { get; set; }
        public string SourceRoot { get; set; }
        public string PackageRoot { get; set; }
        public string FilesRoot { get; set; }
        public string ManifestPath { get; set; }
        public string VersionFilePath { get; set; }
        public string RuntimeVersionFilePath { get; set; }
        public string RuntimeManifestFilePath { get; set; }
        public string RuntimeHashFilePath { get; set; }
        public int CopiedCount { get; set; }
        public int ScannedCount { get; set; }
        public bool ScanLimitHit { get; set; }
        public List<string> CollectedFiles { get; set; } = new List<string>();
        public List<string> DestinationFiles { get; } = new List<string>();
    }

    private sealed class CollectorRuleModel
    {
        public readonly HashSet<string> IncludeExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public readonly HashSet<string> ExcludeExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public readonly List<string> IncludeKeywords = new List<string>();
        public readonly List<string> ExcludeKeywords = new List<string>();
    }
}
#endif
