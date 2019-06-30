namespace dotnetCampus.SourceFusion
{
    /// <summary>
    /// 包含静态转换源码时所需的上下文信息。
    /// </summary>
    public class TransformingContext
    {
        /// <summary>
        /// 获取当前执行源码转换时，对于同一源码文件的转换次数。
        /// </summary>
        public int RepeatIndex { get; internal set; }

        public TransformingContext(int repeatIndex)
        {
            RepeatIndex = repeatIndex;
        }
    }
}
