# KMC EventSphere - Distributed Web Ecosystem

A full-scale, multi-tiered software ecosystem designed to manage, distribute, and consume event planning operations. This project showcases a decoupled service-oriented architecture featuring a custom RESTful API backend and multiple client platforms.

---

## 🏗️ System Architecture Overview

The system is split into three interconnected architectural components:

### 1. ⚙️ Core RESTful API (`KMCEventSphere`)
The centralized engine and data layer of the ecosystem. It manages resource endpoints, processes structured data routing, and handles relational entity lookups.
* **Responsibilities:** API routing, JSON payloads, entity relational schema logic, cross-origin communication settings.

### 2. 🌐 Primary Client Platform (`KMCEventSphereWeb`)
The native client application built to fully consume the core API endpoints. It translates raw JSON server arrays into a clean, operational user interface for system workflows.
* **Responsibilities:** HTTP request handshakes, dynamic state rendering, interface navigation, state verification.

### 3. 🔌 3rd-Party Consumer Client (`TechEventsWeb`)
An external sandbox application demonstrating decoupled data consumption. It validates the API’s modular security design by communicating with the endpoints from a separate client context.
* **Responsibilities:** Independent endpoint interaction, custom dataset layout parsing.
