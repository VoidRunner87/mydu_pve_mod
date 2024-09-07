import {Outlet, useNavigate} from "react-router-dom";
import React from "react";
import {AppProvider, DashboardLayout, Navigation, Router} from "@toolpad/core";
import {createTheme} from "@mui/material";
import {Bolt, Checklist, GridOn, Inventory, LensBlur, Reorder, Slideshow, ViewInAr} from "@mui/icons-material";

const NAVIGATION: Navigation = [
    {
        kind: 'header',
        title: 'Spawner',
    },
    {
        segment: 'prefab',
        title: 'Prefabs',
        icon: <Inventory/>,
    },
    {
        segment: 'script',
        title: 'Scripts',
        icon: <Slideshow/>,
    },
    {
        kind: 'divider',
    },
    {
        kind: 'header',
        title: 'Sectors',
    },
    {
        segment: 'sector-encounter',
        title: 'Definitions',
        icon: <GridOn/>,
    },
    {
        segment: 'sector-instance',
        title: 'Instances',
        icon: <LensBlur/>,
    },
    {
        segment: 'sector-instance-3d',
        title: '3D View',
        icon: <ViewInAr/>,
    },
    {
        kind: 'divider',
    },
    {
        kind: 'header',
        title: 'Events',
    },
    {
        segment: 'event-handler',
        title: 'Event Handlers',
        icon: <Bolt/>,
    },
    {
        kind: 'divider',
    },
    {
        kind: 'header',
        title: 'Others',
    },
    {
        segment: 'feature',
        title: 'Features',
        icon: <Checklist/>,
    },
    {
        segment: 'queue',
        title: 'Task Queue',
        icon: <Reorder/>,
    },
];

const theme = createTheme({
    cssVariables: {
        colorSchemeSelector: 'data-toolpad-color-scheme',
    },
    colorSchemes: {light: true, dark: true},
    breakpoints: {
        values: {
            xs: 0,
            sm: 600,
            md: 600,
            lg: 1200,
            xl: 1536,
        },
    },
});

const Dashboard = (props: any) => {

    const [pathname, setPathname] = React.useState(window.location.pathname);
    const navigate = useNavigate();

    const router = React.useMemo<Router>(() => {
        return {
            pathname,
            searchParams: new URLSearchParams(),
            navigate: (path) => {
                setPathname(String(path));
                navigate(path);
            },
        };
    }, [pathname, navigate]);

    return <AppProvider
        branding={{
            title: "Dynamic Encounters"
        }}
        navigation={NAVIGATION}
        router={router}
        theme={theme}
    >
        <DashboardLayout>
            <Outlet />
        </DashboardLayout>
    </AppProvider>
};

export default Dashboard;