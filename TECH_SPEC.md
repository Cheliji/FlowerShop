# FlowerShop Vibe Coding 技术规格文档

## 1. 项目背景
我们正在开发一个在线鲜花商城系统。目前已完成前后端基础框架搭建，现在需要进入功能实现阶段。

## 2. 技术栈（严格遵循，不可擅自更换）
- **后端**: .NET 10 (FlowerShop.Api)
  - Web API 模式，Minimal API 风格
  - 数据库: MySQL 8
  - ORM: 使用 Entity Framework Core（Pomelo.EntityFrameworkCore.MySql）
  - 文档: 继续使用内置的 OpenAPI / Swagger
  - 认证: JWT Bearer 认证
  - 跨域: 开发环境允许前端 localhost 访问
- **前端**: Vue 3 + TypeScript + Vite (flower-shop-web)
  - UI 库: Element Plus（已安装）
  - 状态管理: Pinia（已安装）
  - 路由: Vue Router 5（已安装）
  - HTTP 客户端: Axios（已安装）
  - 样式预处理器: SCSS/Sass（已安装）
  - 代码规范: 保持与现有文件一致的缩进和风格

## 3. 后端 API 设计规范
- 所有 API 返回统一包装格式：
   {
     "code": 200,
     "message": "success",
     "data": { ... }
   }
   失败时 code 为 400/401/404/500，message 描述错误。

- 使用 DTO 进行请求和响应，禁止直接暴露 Entity 模型