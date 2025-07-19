import React from "react";
import ReactDOM from "react-dom/client";
import {Provider} from "react-redux";
import {Toaster} from "react-hot-toast";
import App from "./App";
import {store} from "@/app/store";
import "@/theme/index.css";

ReactDOM.createRoot(document.getElementById("root") as HTMLElement).render(
    <React.StrictMode>
        {/* Provides the Redux store to the entire application */}
        <Provider store={store}>
            <App/>
            {/* Toaster component for displaying notifications */}
            <Toaster
                position="bottom-center"
                toastOptions={{duration: 3000, className: "text-sm font-medium"}}
            />
        </Provider>
    </React.StrictMode>
);