import React from 'react';
import './App.css';
import PrefabPage from "./prefab/prefab-page";
import '@fontsource/roboto/300.css';
import '@fontsource/roboto/400.css';
import '@fontsource/roboto/500.css';
import '@fontsource/roboto/700.css';
import {createBrowserRouter, Navigate, RouterProvider} from "react-router-dom";
import Dashboard from "./dashboard/dashboard";
import {Container, Typography} from "@mui/material";
import ScriptPage from "./script/script-page";
import SectorInstancePage from "./sector-instance/sector-instance-page";

function App() {

    const ErrorPage = () => {
        return (<Container>
            <Typography variant="h1">404</Typography>
        </Container>);
    }

    const router = createBrowserRouter([
        {
            path: '/',
            element: <Dashboard/>,
            errorElement: <ErrorPage/>,
            children: [
                {path: '', element: <Navigate to="prefab" replace />},
                {path: 'prefab', element: <PrefabPage/>},
                {path: 'script', element: <ScriptPage/>},
                {path: 'sector-instance', element: <SectorInstancePage/>},
            ]
        }
    ]);

    return (<RouterProvider router={router}/>);
}

export default App;
