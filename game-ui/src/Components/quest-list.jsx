import {useEffect, useState} from "react";
import {CloseButton, Container, Header, Panel, PanelBody, Title, Tab} from "./panel";
import styled from "styled-components";
import QuestItem from "./quest-item";

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

    const [jobs, setJobs] = useState([]);
    const [expandedMap, setExpandedMap] = useState({});
    const [factionName, setFactionName] = useState("Unknown");
    const [factionId, setFactionId] = useState(0);
    const [error, setError] = useState("");
    const [acceptedQuestMap, setAcceptedQuestMap] = useState({});

    useEffect(() => {

        if (!window.global_resources) {
            return;
        }

        fetchData();

        // const intervalId = setInterval(() => {
        //     window.modApi.refreshNpcQuestList();
        //     fetchData();
        // }, 5000);
        //
        // return () => {
        //     clearInterval(intervalId);
        // };
    }, []);

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
                        setError(`Failed HTTP Status: ${res.status}`);
                        return [];
                    }

                    return res.json();
                }, err => {
                    window.modApi.cb(`Caught Error ${err}`);
                })
                .then(data => {

                    if (!data)
                    {
                        window.modApi.cb(`Invalid Data`);
                        return;
                    }

                    setJobs(data.jobs);
                    setFactionName(data.faction);
                    setFactionId(data.factionId);
                }, err => {
                    window.modApi.cb(`Caught Error 2 ${err}`);
                });
        } catch (e) {
            setError(`Thrown Error ${e}`);
            window.modApi.cb(`Thrown Error ${e}`);
        }
    };

    const handleClose = () => {
        document.getElementById("root").remove();
    };

    const handleSelect = (item) => {
        if (expandedMap[item.id] === true) {
            setExpandedMap({});
        }
        else {
            setExpandedMap({[item.id]: true});
        }
    };

    const handleAccepted = (index, questId) => {
        acceptedQuestMap[questId] = true;
        setAcceptedQuestMap(acceptedQuestMap);
    };

    const questItems = jobs.map((item, index) =>
        <QuestItem key={index}
                   onSelect={() => handleSelect(item)}
                   questId={item.id}
                   rewards={item.rewards}
                   title={item.title}
                   tasks={item.tasks}
                   type={item.type}
                   onAccepted={(questId) => handleAccepted(index, questId)}
                   accepted={acceptedQuestMap[item.id] || item.accepted}
                   expanded={expandedMap[item.id]} />
    );

    return (
        <Container>
            <Panel>
                <Header>
                    <Title>{factionName} Faction Board</Title>
                    <CloseButton onClick={handleClose}>&times;</CloseButton>
                </Header>
                <PanelBody>
                    <CategoryPanel>
                        <TabContainer>
                            <Tab selected={true}>Missions ({jobs.length})</Tab>
                            {/*<Tab>Combat</Tab>*/}
                            {/*<Tab>Package Delivery</Tab>*/}
                            <br/>
                            <Tab>Reputation</Tab>
                            <Tab>Faction Info</Tab>
                        </TabContainer>
                    </CategoryPanel>
                    <ContentPanel>
                        <div>{error}</div>
                        {questItems}
                    </ContentPanel>
                </PanelBody>
            </Panel>
        </Container>
    );
}

export default QuestList;