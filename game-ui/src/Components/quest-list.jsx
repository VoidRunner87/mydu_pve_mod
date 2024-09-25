import {useEffect, useState} from "react";
import {CloseButton, Container, Header, Panel, PanelBody, Title, Tab} from "./panel";
import styled from "styled-components";

const CategoryPanel = styled.div`
    background-color: rgb(13, 24, 28);
    min-width: 200px;
    display: flex;
    align-items: center;
`;

const TabContainer = styled.div`
    width: 100%;
    padding: 0 8px;

    * {
        margin-bottom: 2px;
    }
`;

const ContentPanel = styled.div`
    padding: 16px;
    flex-grow: 1;
`;

const QuestList = (props) => {

    const [items, setItems] = useState([]);
    const [error, setError] = useState("");

    // useEffect(() => {
    //
    //     if (!window.global_resources)
    //     {
    //         return;
    //     }
    //
    //     let url = window.global_resources["faction-quests"];
    //
    //     fetch(url)
    //         .then(res => {
    //             setItems(res.json());
    //         });
    //
    // }, [setItems]);

    const fetchData = () => {

        try {
            if (!window.global_resources) {
                setError("Null Global Resources");
                return;
            }

            let url = window.global_resources["faction-quests"];

            fetch(url)
                .then(res => {
                    if (!res.ok) {
                        setError("Failed HTTP Status");
                        return [];
                    }

                    return res.json();
                })
                .then(data => setItems(data));
        }
        catch (e)
        {
            setError(`Thrown Error ${e}`);
        }
    };

    const handleClose = () => {
        document.getElementById("root").remove();
    };

    const itemElements = items.map((item, index) =>
        <div key={index}>{item.title}</div>
    );

    return (
        <Container>
            <Panel>
                <Header>
                    <Title>Red Talon Enclave Jobs</Title>
                    <CloseButton onClick={handleClose}>&times;</CloseButton>
                </Header>
                <PanelBody>
                    <CategoryPanel>
                        <TabContainer>
                            <Tab onClick={fetchData} selected={true}>Combat</Tab>
                            <Tab>Package Delivery</Tab>
                            <Tab>Reputation</Tab>
                            <Tab>Faction Info</Tab>
                        </TabContainer>
                    </CategoryPanel>
                    <ContentPanel>
                        <div>{error}</div>
                        {itemElements}
                    </ContentPanel>
                </PanelBody>
            </Panel>
        </Container>
    );
}

export default QuestList;