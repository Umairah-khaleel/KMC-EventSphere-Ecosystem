# KMC EventSphere - Distributed Web Ecosystem

A full-scale, multi-tiered software ecosystem designed to manage, distribute, and consume event planning workflows. This project showcases a completely decoupled, service-oriented architecture featuring a custom C# ASP.NET Core RESTful API backend and multiple consumer client platforms.

---

## 🏗️ System Architecture Overview

The ecosystem is divided into three core interconnected applications:

### 1. ⚙️ Core RESTful API Backend (`KMCEventSphere`)
The centralized data orchestration engine of the ecosystem built with ASP.NET Core. It manages server endpoints, structures relational database queries, handles request-routing logic, and services JSON data streams.
* **Key Focus:** API endpoint mapping, JSON payload generation, database connectivity, and structured resource management using a clean Controller/Model architecture.

### 2. 🌐 Primary Consumer Client (`KMCEventSphereWeb`)
The native web interface built to fully consume the core API endpoints. It dynamically communicates with the backend services via HTTP handshakes to parse raw server data arrays into an interactive, functional operational dashboard.
* **Key Focus:** HTTP communication, real-time UI rendering, and internal platform workflow state tracking.

### 3. 🔌 3rd-Party Integration Client (`TechEventsWeb`)
An independent application acting as an external sandbox consumer. It validates the API's cross-origin data sharing capabilities and structural security design by securely pulling and parsing endpoints from a completely separate client context.
* **Key Focus:** Decoupled data consumption, custom layout rendering, and external endpoint interaction.
