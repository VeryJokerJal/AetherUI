# AetherUI Framework

AetherUI是一个基于C#的现代UI框架编译器，参考WPF和Avalonia的架构设计，使用OpenTK进行渲染。

## 项目架构

### 模块化设计

- **AetherUI.Core** - 核心框架模块
  - MVVM基础设施（ViewModelBase、RelayCommand等）
  - 依赖属性系统
  - 基础类型定义

- **AetherUI.Events** - 事件系统模块
  - 路由事件实现
  - 事件捕获和冒泡阶段
  - 命令绑定机制

- **AetherUI.Layout** - 布局系统模块
  - StackPanel：垂直/水平堆叠布局
  - Grid：网格布局系统
  - Canvas：绝对定位画布布局
  - DockPanel：停靠面板布局
  - WrapPanel：自动换行布局
  - UniformGrid：统一网格布局
  - Card：卡片容器布局
  - Border：边框装饰容器

- **AetherUI.Rendering** - 渲染模块
  - OpenTK集成
  - 渲染管道
  - 几何变换和材质渲染

- **AetherUI.Markup** - 标记语言解析模块
  - XAML解析器
  - JSON配置解析器
  - 自定义DSL解析器

- **AetherUI.Compiler** - 编译器模块
  - 标记语言编译
  - 代码生成
  - 热重载支持

- **AetherUI.Designer** - 设计时支持模块
  - 设计时数据绑定
  - 预览功能

## 技术特性

- **MVVM模式**：完整的MVVM架构支持
- **多标记语言**：支持XAML、JSON和自定义DSL
- **OpenTK渲染**：高性能的OpenGL渲染
- **热重载**：实时预览功能
- **模块化**：松耦合的模块化设计

## 开发规范

- 使用显式类型声明，避免使用var关键字
- 调试信息使用Debug.WriteLine输出
- UI文本不包含表情符号
- 每个功能模块完成后创建Git提交记录
