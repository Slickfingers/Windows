// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Tests;
using CommunityToolkit.Tooling.TestGen;
using CommunityToolkit.WinUI.Automation.Peers;
using CommunityToolkit.WinUI.Controls;

namespace TokenizingTextBoxTests;

[TestClass]
[TestCategory("Test_TokenizingTextBox")]
public partial class Test_TokenizingTextBox_AutomationPeer : VisualUITestBase
{
    [TestMethod]
    public async Task ShouldConfigureTokenizingTextBoxAutomationPeerAsync()
    {
        await App.DispatcherQueue.EnqueueAsync(async () =>
        {
            const string expectedAutomationName = "MyAutomationName";
            const string expectedName = "MyName";
            const string expectedValue = "Wor";

            var items = new ObservableCollection<TokenizingTextBoxTestItem> { new() { Title = "Hello" }, new() { Title = "World" } };

            var tokenizingTextBox = new TokenizingTextBox { ItemsSource = items };

            await LoadTestContentAsync(tokenizingTextBox);

            var tokenizingTextBoxAutomationPeer =
                FrameworkElementAutomationPeer.CreatePeerForElement(tokenizingTextBox) as TokenizingTextBoxAutomationPeer;

            Assert.IsNotNull(tokenizingTextBoxAutomationPeer, "Verify that the AutomationPeer is TokenizingTextBoxAutomationPeer.");

            // Asserts the automation peer name based on the Automation Property Name value. 
            tokenizingTextBox.SetValue(AutomationProperties.NameProperty, expectedAutomationName);
            Assert.IsTrue(tokenizingTextBoxAutomationPeer.GetName().Contains(expectedAutomationName), "Verify that the UIA name contains the given AutomationProperties.Name of the TokenizingTextBox.");

            // Asserts the automation peer name based on the element Name property.
            tokenizingTextBox.Name = expectedName;
            Assert.IsTrue(tokenizingTextBoxAutomationPeer.GetName().Contains(expectedName), "Verify that the UIA name contains the given Name of the TokenizingTextBox.");

            tokenizingTextBoxAutomationPeer.SetValue(expectedValue);
            Assert.IsTrue(tokenizingTextBoxAutomationPeer.Value.Equals(expectedValue), "Verify that the Value contains the given Text of the TokenizingTextBox.");
        });
    }

    [TestMethod]
    public async Task ShouldReturnTokensForTokenizingTextBoxAutomationPeerAsync()
    {
        await App.DispatcherQueue.EnqueueAsync(async () =>
        {
            var items = new ObservableCollection<TokenizingTextBoxTestItem>
            {
                    new() { Title = "Hello" }, new() { Title = "World" }
            };

            var tokenizingTextBox = new TokenizingTextBox { ItemsSource = items };

            await LoadTestContentAsync(tokenizingTextBox);

            tokenizingTextBox
                .SelectAllTokensAndText(); // Will be 3 items due to the `AndText` that will select an empty text item.
            var tokenizingTextBoxAutomationPeer =
                FrameworkElementAutomationPeer.CreatePeerForElement(tokenizingTextBox) as
                    TokenizingTextBoxAutomationPeer;

            Assert.IsNotNull(
                tokenizingTextBoxAutomationPeer,
                "Verify that the AutomationPeer is TokenizingTextBoxAutomationPeer.");

            var selectedItems = tokenizingTextBoxAutomationPeer
                .GetChildren()
                .Cast<ListViewItemAutomationPeer>()
                .Select(peer => peer.Owner as TokenizingTextBoxItem)
                .Select(item => item?.Content as TokenizingTextBoxTestItem)
                .ToList();

            Assert.AreEqual(3, selectedItems.Count);
            Assert.AreEqual(items[0], selectedItems[0]);
            Assert.AreEqual(items[1], selectedItems[1]);
            Assert.IsNull(selectedItems[2]); // The 3rd item is the empty text item.
        });
    }

    [UIThreadTestMethod]
    public async Task ShouldThrowElementNotEnabledExceptionIfValueSetWhenDisabled()
    {
        const string expectedValue = "Wor";

        var tokenizingTextBox = new TokenizingTextBox { IsEnabled = false };

        await LoadTestContentAsync(tokenizingTextBox);

        var tokenizingTextBoxAutomationPeer =
            FrameworkElementAutomationPeer.CreatePeerForElement(tokenizingTextBox) as TokenizingTextBoxAutomationPeer;

        Assert.ThrowsException<ElementNotEnabledException>(() =>
        {
            tokenizingTextBoxAutomationPeer!.SetValue(expectedValue);
        });
    }

    public class TokenizingTextBoxTestItem
    {
        public string? Title { get; set; }

        public override string ToString()
        {
            return Title!;
        }
    }
}
